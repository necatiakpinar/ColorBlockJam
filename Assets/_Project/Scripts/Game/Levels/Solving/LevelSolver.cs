using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Solving
{
    public sealed class LevelSolver
    {
        const char GoneKey = char.MaxValue;

        readonly int _stateCap;

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
            public int[] Positions;
            public bool[] Exited;
            public Node Parent;
            public List<SolveStep> Steps;
            public int Rounds;
            public int Moves;
        }

        public SolveResult Solve(LevelData level) => Solve(level, CancellationToken.None);

        public SolveResult Solve(LevelData level, CancellationToken token) => Solve(level, int.MaxValue, token);

        public SolveResult Solve(LevelData level, int maxMoves, CancellationToken token)
        {
            Prepare(level);
            int count = level.Blocks.Count;
            var start = new Node { Positions = new int[count], Exited = new bool[count], Steps = new List<SolveStep>() };
            for (int id = 0; id < count; id++)
                start.Positions[id] = _board.StartPosition(id);
            start.Rounds = LeaveWhilePossible(start.Positions, start.Exited, start.Steps);

            var visited = new HashSet<string>(KeyComparer.Instance) { Key(start.Positions, start.Exited) };
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
