using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Solving
{
    /// <summary>What <see cref="LevelSolver"/> found out about a level.</summary>
    public enum SolveStatus
    {
        Solvable = 1000,
        Unsolvable = 2000,

        /// <summary>The search hit its state cap before it could decide.</summary>
        Unknown = 3000,
    }

    /// <summary>
    /// One step of a solution: block <see cref="BlockId"/> goes to <see cref="Offset"/> (from where the level
    /// placed it) along free cells; when <see cref="Exits"/> it then leaves through the door in
    /// <see cref="ExitDirection"/>.
    /// </summary>
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

        /// <summary>A shortest solution in moves (exits follow as soon as they can); empty unless solvable.</summary>
        public IReadOnlyList<SolveStep> Steps { get; }

        public int StatesSearched { get; }

        /// <summary>
        /// Waves of blocks leaving along the solution: blocks that can go at the same moment form one wave, so a
        /// block that waits for others adds a wave (DA-33).
        /// </summary>
        public int Rounds { get; }

        /// <summary>The level's difficulty measure (DA-33): exit waves plus room-making moves.</summary>
        public int Score => Rounds + Moves;

        /// <summary>Moves that only make room: a block repositioned without leaving.</summary>
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
