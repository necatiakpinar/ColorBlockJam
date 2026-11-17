using System.Collections.Generic;
using System.Text;
using ColorBlockJam.Game.Gameplay.Board;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Solving
{
    /// <summary>
    /// Decides whether a level can be finished (T-33, DA-32), with the game's own rules: it lays board states
    /// out on a real <see cref="BoardState"/>. A move takes one block along any path of free cells, as the drag
    /// allows. Leaving never hurts (it only frees cells), so every block that can reach its door leaves at once,
    /// in waves: each wave is every block that can leave at that moment. The search only branches over moves
    /// that make room, breadth first, so the solution it returns has the fewest such moves. Editor-time code:
    /// clarity over allocation.
    /// </summary>
    public sealed class LevelSolver
    {
        static readonly Vector2Int[] Directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        readonly int _stateCap;

        public LevelSolver(int stateCap)
        {
            _stateCap = stateCap;
        }

        sealed class Node
        {
            public Vector2Int[] Offsets;
            public bool[] Exited;
            public Node Parent;
            public List<SolveStep> Steps;
            // Exit waves from the start up to this node.
            public int Rounds;
        }

        /// <summary>The level must pass the validator's errors first; the solver assumes a buildable board.</summary>
        public SolveResult Solve(LevelData level)
        {
            var board = new BoardState(level);
            int count = level.Blocks.Count;
            var start = new Node { Offsets = new Vector2Int[count], Exited = new bool[count], Steps = new List<SolveStep>() };
            start.Rounds = LeaveWhilePossible(board, start.Offsets, start.Exited, start.Steps);

            var visited = new HashSet<string> { Key(start) };
            var queue = new Queue<Node>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();
                if (AllExited(node.Exited))
                    return new SolveResult(SolveStatus.Solvable, Path(node), visited.Count, node.Rounds);

                for (int id = 0; id < count; id++)
                {
                    if (node.Exited[id])
                        continue;
                    board.Arrange(node.Offsets, node.Exited);
                    foreach (Vector2Int target in Reachable(board, id))
                    {
                        if (target == node.Offsets[id])
                            continue;
                        var child = new Node
                        {
                            Offsets = (Vector2Int[])node.Offsets.Clone(),
                            Exited = (bool[])node.Exited.Clone(),
                            Parent = node,
                            Steps = new List<SolveStep> { new(id, target, false, Vector2Int.zero) },
                        };
                        child.Offsets[id] = target;
                        child.Rounds = node.Rounds + LeaveWhilePossible(board, child.Offsets, child.Exited, child.Steps);
                        if (!visited.Add(Key(child)))
                            continue;
                        if (visited.Count >= _stateCap)
                            return new SolveResult(SolveStatus.Unknown, new List<SolveStep>(), visited.Count, 0);
                        queue.Enqueue(child);
                    }

                    board.Arrange(node.Offsets, node.Exited);
                }
            }

            return new SolveResult(SolveStatus.Unsolvable, new List<SolveStep>(), visited.Count, 0);
        }

        /// <summary>
        /// Lets blocks leave in waves until none can: each wave takes every block that can reach its door on the
        /// board as it stands. Returns how many waves there were.
        /// </summary>
        int LeaveWhilePossible(BoardState board, Vector2Int[] offsets, bool[] exited, List<SolveStep> steps)
        {
            int waves = 0;
            var leaving = new List<SolveStep>();
            while (true)
            {
                leaving.Clear();
                board.Arrange(offsets, exited);
                for (int id = 0; id < offsets.Length; id++)
                    if (!exited[id] && TryFindExit(board, id, out SolveStep exit))
                        leaving.Add(exit);
                if (leaving.Count == 0)
                    return waves;
                foreach (SolveStep exit in leaving)
                {
                    offsets[exit.BlockId] = exit.Offset;
                    exited[exit.BlockId] = true;
                }

                steps.AddRange(leaving);
                waves++;
            }
        }

        /// <summary>Whether the block can reach a spot from which it leaves; the board is left as it was.</summary>
        bool TryFindExit(BoardState board, int id, out SolveStep exit)
        {
            Vector2Int origin = board.Blocks[id].Offset;
            foreach (Vector2Int position in Reachable(board, id))
            {
                board.MoveBlockTo(id, position);
                foreach (Vector2Int direction in Directions)
                {
                    if (board.CanStep(id, direction) != StepResult.Exited)
                        continue;
                    board.MoveBlockTo(id, origin);
                    exit = new SolveStep(id, position, true, direction);
                    return true;
                }
            }

            board.MoveBlockTo(id, origin);
            exit = default;
            return false;
        }

        /// <summary>Every offset the block can reach through free cells from where it is, its own included.</summary>
        List<Vector2Int> Reachable(BoardState board, int id)
        {
            Vector2Int origin = board.Blocks[id].Offset;
            var reached = new List<Vector2Int> { origin };
            var seen = new HashSet<Vector2Int> { origin };
            for (int i = 0; i < reached.Count; i++)
            {
                board.MoveBlockTo(id, reached[i]);
                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int next = reached[i] + direction;
                    if (!seen.Contains(next) && board.CanStep(id, direction) == StepResult.Moved && seen.Add(next))
                        reached.Add(next);
                }
            }

            board.MoveBlockTo(id, origin);
            return reached;
        }

        bool AllExited(bool[] exited)
        {
            foreach (bool gone in exited)
                if (!gone)
                    return false;
            return true;
        }

        string Key(Node node)
        {
            var key = new StringBuilder();
            for (int i = 0; i < node.Offsets.Length; i++)
                key.Append(node.Exited[i] ? "x;" : $"{node.Offsets[i].x},{node.Offsets[i].y};");
            return key.ToString();
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
