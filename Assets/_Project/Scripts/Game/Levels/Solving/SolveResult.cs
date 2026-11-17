using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Solving
{
    public enum SolveStatus
    {
        Solvable = 1000,
        Unsolvable = 2000,

        Unknown = 3000,
    }

    public readonly struct SolveStep
    {
        public readonly int BlockId;
        public readonly Vector2Int Offset;
        public readonly bool Exits;
        public readonly Vector2Int ExitDirection;

        public SolveStep(int blockId, Vector2Int offset, bool exits, Vector2Int exitDirection)
        {
            BlockId = blockId;
            Offset = offset;
            Exits = exits;
            ExitDirection = exitDirection;
        }
    }

    public sealed class SolveResult
    {
        public SolveResult(SolveStatus status, IReadOnlyList<SolveStep> steps, int statesSearched, int rounds)
        {
            Status = status;
            Steps = steps;
            StatesSearched = statesSearched;
            Rounds = rounds;
        }

        public SolveStatus Status { get; }

        public IReadOnlyList<SolveStep> Steps { get; }

        public int StatesSearched { get; }

        public int Rounds { get; }

        public int Score => Rounds + Moves;

        public int Moves
        {
            get
            {
                int moves = 0;
                for (int i = 0; i < Steps.Count; i++)
                    if (!Steps[i].Exits)
                        moves++;
                return moves;
            }
        }
    }
}
