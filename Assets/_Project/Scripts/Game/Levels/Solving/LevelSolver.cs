using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Solving
{
    /// <summary>
    /// Decides whether a level can be finished (T-33, DA-32), with the game's own rules, read once into a
    /// <see cref="SearchBoard"/> so each search step is a few bit operations. A move takes one block along any
    /// path of free cells, as the drag allows. Leaving never hurts (it only frees cells), so every block that can
    /// reach its door leaves at once, in waves: each wave is every block that can leave at that moment. The search
    /// only branches over moves that make room, breadth first, so the solution it returns has the fewest such
    /// moves. It keeps search buffers between calls, so use one instance per thread.
    /// </summary>
    public sealed class LevelSolver
    {
        const char GoneKey = char.MaxValue;

        readonly int _stateCap;

        // Reused buffers: the search visits thousands of states and must not allocate per state for them.
        readonly List<int> _targets = new();
        readonly List<int> _reach = new();
        readonly List<SolveStep> _leaving = new();
        readonly List<int> _leavingAt = new();
        int[] _seen = Array.Empty<int>();
        int _seenMark;
        char[] _key = Array.Empty<char>();
        SearchBoard _board;

        public LevelSolver(int stateCap)
        {
            _stateCap = stateCap;
        }

        sealed class Node
        {
            // Each block's position on the SearchBoard; meaningless once it has exited.
            public int[] Positions;
            public bool[] Exited;
            public Node Parent;
            public List<SolveStep> Steps;
            // Exit waves from the start up to this node.
            public int Rounds;
            // Room-making moves from the start up to this node: its depth in the search.
            public int Moves;
        }

        /// <summary>The level must pass the validator's errors first; the solver assumes a buildable board.</summary>
        public SolveResult Solve(LevelData level) => Solve(level, CancellationToken.None);

        /// <summary>
        /// As <see cref="Solve(LevelData)"/>, stopping with <see cref="OperationCanceledException"/> once
        /// <paramref name="token"/> is cancelled: the level editor solves on a worker thread and drops a search the
        /// next edit makes stale.
        /// </summary>
        public SolveResult Solve(LevelData level, CancellationToken token) => Solve(level, int.MaxValue, token);

        /// <summary>
        /// As <see cref="Solve(LevelData, CancellationToken)"/>, trying only solutions with at most
        /// <paramref name="maxMoves"/> room-making moves: the generator skips what could never score low enough.
        /// A level that needs more says <see cref="SolveStatus.Unknown"/>.
        /// </summary>
        public SolveResult Solve(LevelData level, int maxMoves, CancellationToken token)
        {
            Prepare(level);
            int count = level.Blocks.Count;
            var start = new Node { Positions = new int[count], Exited = new bool[count], Steps = new List<SolveStep>() };
            for (int id = 0; id < count; id++)
                start.Positions[id] = _board.StartPosition(id);
            start.Rounds = LeaveWhilePossible(start.Positions, start.Exited, start.Steps);

            var visited = new HashSet<string>(KeyComparer.Instance) { Key(start.Positions, start.Exited) };
            // Boards right after a move, before anyone leaves: the same one always ends the same way, so it is
            // played out once, however many moves reach it.
            var played = new HashSet<string>(KeyComparer.Instance);
            var queue = new Queue<Node>();
            queue.Enqueue(start);
            bool cutShort = false;
            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();
                Node node = queue.Dequeue();
                if (AllExited(node.Exited))
                    return new SolveResult(SolveStatus.Solvable, Path(node), visited.Count, node.Rounds);
                if (node.Moves >= maxMoves)
                {
                    cutShort = true;
                    continue;
                }

                Occupancy(node.Positions, node.Exited, out ulong low, out ulong high, out int gone);
                for (int id = 0; id < count; id++)
                {
                    if (node.Exited[id])
                        continue;
                    int from = node.Positions[id];
                    Reachable(id, from, low & ~_board.Low(id, from), high & ~_board.High(id, from), gone, _targets);
                    for (int t = 1; t < _targets.Count; t++)
                    {
                        int target = _targets[t];
                        var positions = (int[])node.Positions.Clone();
                        positions[id] = target;
                        if (!played.Add(Key(positions, node.Exited)))
                            continue;
                        var child = new Node
                        {
                            Positions = positions,
                            Exited = (bool[])node.Exited.Clone(),
                            Parent = node,
                            Steps = new List<SolveStep> { new(id, _board.OffsetOf(id, target), false, Vector2Int.zero) },
                            Moves = node.Moves + 1,
                        };
                        child.Rounds = node.Rounds + LeaveWhilePossible(child.Positions, child.Exited, child.Steps);
                        if (!visited.Add(Key(child.Positions, child.Exited)))
                            continue;
                        if (visited.Count >= _stateCap)
                            return new SolveResult(SolveStatus.Unknown, new List<SolveStep>(), visited.Count, 0);
                        queue.Enqueue(child);
                    }
                }
            }

            return new SolveResult(cutShort ? SolveStatus.Unknown : SolveStatus.Unsolvable, new List<SolveStep>(), visited.Count, 0);
        }

        void Prepare(LevelData level)
        {
            _board = new SearchBoard(level);
            int most = 0;
            for (int id = 0; id < _board.BlockCount; id++)
                most = Math.Max(most, _board.PositionCount(id));
            if (_seen.Length < most)
                _seen = new int[most];
            if (_key.Length != _board.BlockCount)
                _key = new char[_board.BlockCount];
        }

        /// <summary>The cells every block still on the board covers, and how many have left.</summary>
        void Occupancy(int[] positions, bool[] exited, out ulong low, out ulong high, out int gone)
        {
            low = 0;
            high = 0;
            gone = 0;
            for (int id = 0; id < positions.Length; id++)
            {
                if (exited[id])
                {
                    gone++;
                    continue;
                }

                low |= _board.Low(id, positions[id]);
                high |= _board.High(id, positions[id]);
            }
        }

        /// <summary>
        /// Lets blocks leave in waves until none can: each wave takes every block that can reach its door on the
        /// board as it stands. Returns how many waves there were.
        /// </summary>
        int LeaveWhilePossible(int[] positions, bool[] exited, List<SolveStep> steps)
        {
            int waves = 0;
            while (true)
            {
                _leaving.Clear();
                _leavingAt.Clear();
                Occupancy(positions, exited, out ulong low, out ulong high, out int gone);
                for (int id = 0; id < positions.Length; id++)
                {
                    if (exited[id])
                        continue;
                    int at = positions[id];
                    if (TryFindExit(id, at, low & ~_board.Low(id, at), high & ~_board.High(id, at), gone, out SolveStep exit, out int from))
                    {
                        _leaving.Add(exit);
                        _leavingAt.Add(from);
                    }
                }

                if (_leaving.Count == 0)
                    return waves;
                for (int i = 0; i < _leaving.Count; i++)
                {
                    positions[_leaving[i].BlockId] = _leavingAt[i];
                    exited[_leaving[i].BlockId] = true;
                }

                steps.AddRange(_leaving);
                waves++;
            }
        }

        /// <summary>Whether the block can reach a spot from which it leaves, the others standing still.</summary>
        bool TryFindExit(int id, int position, ulong othersLow, ulong othersHigh, int gone, out SolveStep exit, out int from)
        {
            Reachable(id, position, othersLow, othersHigh, gone, _reach);
            for (int r = 0; r < _reach.Count; r++)
                for (int d = 0; d < SearchBoard.Directions.Length; d++)
                {
                    if (!_board.Allows(id, gone, d) || !_board.CanLeave(id, _reach[r], d, othersLow, othersHigh))
                        continue;
                    from = _reach[r];
                    exit = new SolveStep(id, _board.OffsetOf(id, from), true, SearchBoard.Directions[d]);
                    return true;
                }

            exit = default;
            from = -1;
            return false;
        }

        /// <summary>Fills <paramref name="reached"/> with every position the block can reach through free cells, its own first.</summary>
        void Reachable(int id, int position, ulong othersLow, ulong othersHigh, int gone, List<int> reached)
        {
            reached.Clear();
            reached.Add(position);
            _seenMark++;
            _seen[position] = _seenMark;
            for (int i = 0; i < reached.Count; i++)
                for (int d = 0; d < SearchBoard.Directions.Length; d++)
                {
                    int next = _board.MoveTo(id, reached[i], d);
                    if (next < 0 || _seen[next] == _seenMark || !_board.Allows(id, gone, d) ||
                        (_board.Low(id, next) & othersLow) != 0 || (_board.High(id, next) & othersHigh) != 0)
                        continue;
                    _seen[next] = _seenMark;
                    reached.Add(next);
                }
        }

        bool AllExited(bool[] exited)
        {
            foreach (bool gone in exited)
                if (!gone)
                    return false;
            return true;
        }

        string Key(int[] positions, bool[] exited)
        {
            for (int i = 0; i < positions.Length; i++)
                _key[i] = exited[i] ? GoneKey : (char)positions[i];
            return new string(_key);
        }

        /// <summary>
        /// Keys hold small position numbers, which the runtime's string hash spreads badly enough to make the sets
        /// crawl on some levels; this one mixes every character in.
        /// </summary>
        sealed class KeyComparer : IEqualityComparer<string>
        {
            public static readonly KeyComparer Instance = new();

            public bool Equals(string a, string b) => string.Equals(a, b);

            public int GetHashCode(string key)
            {
                ulong hash = 14695981039346656037UL;
                for (int i = 0; i < key.Length; i++)
                    hash = (hash ^ key[i]) * 1099511628211UL;
                hash ^= hash >> 29;
                return (int)hash ^ (int)(hash >> 32);
            }
        }

        List<SolveStep> Path(Node node)
        {
            var chain = new List<Node>();
            for (Node at = node; at != null; at = at.Parent)
                chain.Add(at);
            chain.Reverse();
            var steps = new List<SolveStep>();
            foreach (Node at in chain)
                steps.AddRange(at.Steps);
            return steps;
        }
    }
}
