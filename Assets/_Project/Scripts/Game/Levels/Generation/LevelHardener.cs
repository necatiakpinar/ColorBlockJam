using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ColorBlockJam.Game.Levels.Blockers;
using ColorBlockJam.Game.Levels.Solving;
using UnityEngine;
using Random = System.Random;

namespace ColorBlockJam.Game.Levels.Generation
{
    public sealed class LevelHardener
    {
        static readonly ArrowAxis[] Axes = { ArrowAxis.None, ArrowAxis.Horizontal, ArrowAxis.Vertical };
        const int PlacementTries = 30;
        const int MaxIce = 3;
        const int ProposalsPerRound = 16;

        readonly LevelDefinitionValidator _validator;
        readonly int _solverStateCap;

        public LevelHardener(LevelDefinitionValidator validator, int solverStateCap)
        {
            _validator = validator;
            _solverStateCap = solverStateCap;
        }

        public LevelHardeningResult Harden(LevelData level, LevelHardeningRequest request, CancellationToken token = default)
        {
            SolveResult current = Evaluate(level, new LevelSolver(_solverStateCap), token);
            if (current == null)
                return null;

            var random = new Random(request.Seed);
            int batch = ProposalsPerRound;
            var proposals = new LevelData[batch];
            var results = new SolveResult[batch];
            var options = new ParallelOptions { CancellationToken = token };
            int rounds = 0;
            while (rounds < request.Rounds && current.Moves < request.TargetMoves)
            {
                rounds++;
                for (int i = 0; i < batch; i++)
                    proposals[i] = Propose(level, request, random);
                Parallel.For(0, batch, options, () => new LevelSolver(_solverStateCap),
                    (i, _, solver) =>
                    {
                        results[i] = proposals[i] == null ? null : Evaluate(proposals[i], solver, token);
                        return solver;
                    },
                    _ => { });

                int best = -1;
                for (int i = 0; i < batch; i++)
                    if (results[i] != null && Better(results[i], best < 0 ? current : results[best], best < 0))
                        best = i;
                if (best < 0)
                    continue;
                level = proposals[best];
                current = results[best];
            }

            return new LevelHardeningResult(level, current, rounds);
        }

        static bool Better(SolveResult candidate, SolveResult than, bool thanIsCurrent)
        {
            if (candidate.Moves != than.Moves)
                return candidate.Moves > than.Moves;
            if (candidate.Rounds != than.Rounds)
                return candidate.Rounds > than.Rounds;
            return thanIsCurrent;
        }

        SolveResult Evaluate(LevelData level, LevelSolver solver, CancellationToken token)
        {
            if (_validator.HasErrors(_validator.Validate(level)))
                return null;
            SolveResult solution = solver.Solve(level, token);
            return solution.Status == SolveStatus.Solvable ? solution : null;
        }

        LevelData Propose(LevelData level, LevelHardeningRequest request, Random random)
        {
            var blocks = new List<BlockData>(level.Blocks);
            var doors = new List<DoorData>(level.Doors);
            int kind = random.Next(100);
            if (kind < 55)
            {
                if (!TryMoveBlock(level, blocks, random))
                    return null;
            }
            else if (kind < 70)
            {
                if (!TrySlideDoor(level, doors, random))
                    return null;
            }
            else if (kind < 85)
            {
                if (!request.UseArrows)
                    return null;
                int id = random.Next(blocks.Count);
                BlockerSet blockers = blocks[id].Blockers.Clone();
                blockers.Arrow.Axis = Axes[random.Next(Axes.Length)];
                blocks[id] = new BlockData(blocks[id].Color, blocks[id].Cells, blockers);
            }
            else
            {
                if (!request.UseIce)
                    return null;
                int id = random.Next(blocks.Count);
                BlockerSet blockers = blocks[id].Blockers.Clone();
                blockers.Ice.Exits = random.Next(Math.Min(MaxIce, blocks.Count - 1) + 1);
                blocks[id] = new BlockData(blocks[id].Color, blocks[id].Cells, blockers);
            }

            return new LevelData(level.Width, level.Height, level.TimerSeconds, blocks, doors, level.Difficulty);
        }

        bool TryMoveBlock(LevelData level, List<BlockData> blocks, Random random)
        {
            int id = random.Next(blocks.Count);
            var taken = new HashSet<Vector2Int>();
            for (int i = 0; i < blocks.Count; i++)
                if (i != id)
                    taken.UnionWith(blocks[i].Cells);
            IReadOnlyList<Vector2Int> cells = blocks[id].Cells;
            for (int tries = 0; tries < PlacementTries; tries++)
            {
                var shift = new Vector2Int(random.Next(-level.Width + 1, level.Width), random.Next(-level.Height + 1, level.Height));
                if (shift == Vector2Int.zero)
                    continue;
                var moved = new List<Vector2Int>(cells.Count);
                bool fits = true;
                foreach (Vector2Int cell in cells)
                {
                    Vector2Int at = cell + shift;
                    fits &= at.x >= 0 && at.y >= 0 && at.x < level.Width && at.y < level.Height && !taken.Contains(at);
                    moved.Add(at);
                }

                if (!fits)
                    continue;
                blocks[id] = new BlockData(blocks[id].Color, moved, blocks[id].Blockers.Clone());
                return true;
            }

            return false;
        }

        bool TrySlideDoor(LevelData level, List<DoorData> doors, Random random)
        {
            int id = random.Next(doors.Count);
            DoorData door = doors[id];
            int wall = door.Side is WallSide.Bottom or WallSide.Top ? level.Width : level.Height;
            for (int tries = 0; tries < PlacementTries; tries++)
            {
                var slid = new DoorData(door.Side, random.Next(wall - door.Length + 1), door.Length, door.Color);
                if (slid.Start == door.Start || Overlaps(slid, doors, id))
                    continue;
                doors[id] = slid;
                return true;
            }

            return false;
        }

        static bool Overlaps(DoorData door, List<DoorData> doors, int except)
        {
            for (int i = 0; i < doors.Count; i++)
                if (i != except && doors[i].Side == door.Side && doors[i].Start <= door.End && doors[i].End >= door.Start)
                    return true;
            return false;
        }
    }

    public readonly struct LevelHardeningRequest
    {
        public LevelHardeningRequest(int seed, int rounds, int targetMoves, bool useArrows, bool useIce)
        {
            Seed = seed;
            Rounds = rounds;
            TargetMoves = targetMoves;
            UseArrows = useArrows;
            UseIce = useIce;
        }

        public int Seed { get; }

        public int Rounds { get; }

        public int TargetMoves { get; }

        public bool UseArrows { get; }
        public bool UseIce { get; }
    }

    public sealed class LevelHardeningResult
    {
        public LevelHardeningResult(LevelData level, SolveResult solution, int rounds)
        {
            Level = level;
            Solution = solution;
            Rounds = rounds;
        }

        public LevelData Level { get; }
        public SolveResult Solution { get; }
        public int Rounds { get; }
    }
}
