using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Levels.Solving;
using UnityEngine;
using Random = System.Random;

namespace ColorBlockJam.Game.Levels.Generation
{
    /// <summary>
    /// Builds a level from a <see cref="LevelGenerationRequest"/> (T-47, DA-33). Each candidate gets one door per
    /// colour on the wall ring and preset-shaped blocks in those colours, each one able to fit a door of its
    /// colour. A candidate is kept only when the validator finds no error and the solver proves it finishable;
    /// its difficulty is the solution's score: exit waves plus room-making moves. Editor-time code.
    /// </summary>
    public sealed class LevelGenerator
    {
        static readonly WallSide[] Sides = { WallSide.Bottom, WallSide.Top, WallSide.Left, WallSide.Right };
        static readonly BlockColor[] Palette = (BlockColor[])Enum.GetValues(typeof(BlockColor));
        // Per-candidate placement retries: enough to fill a board without looping on an impossible one.
        const int PlacementTries = 40;
        const int MaxDoorLength = 3;

        readonly LevelDefinitionValidator _validator;
        readonly LevelSolver _solver;
        readonly BlockShapeLibrary _shapes;

        public LevelGenerator(LevelDefinitionValidator validator, LevelSolver solver, BlockShapeLibrary shapes)
        {
            _validator = validator;
            _solver = solver;
            _shapes = shapes;
        }

        /// <summary>The lowest and highest solution score at each difficulty (DA-33).</summary>
        public (int Min, int Max) ScoreFor(LevelDifficulty difficulty) => difficulty switch
        {
            LevelDifficulty.Normal => (1, 2),
            LevelDifficulty.Hard => (3, 4),
            _ => (5, int.MaxValue),
        };

        /// <summary>20 s plus 10 s a block, plus 30 % for Hard and 60 % for Very Hard (DA-33).</summary>
        public int TimerFor(int blocks, LevelDifficulty difficulty)
        {
            float seconds = 20 + 10 * blocks;
            seconds *= difficulty switch
            {
                LevelDifficulty.Hard => 1.3f,
                LevelDifficulty.VeryHard => 1.6f,
                _ => 1f,
            };
            return Mathf.RoundToInt(seconds / 5f) * 5;
        }

        public LevelGenerationResult Generate(LevelGenerationRequest request)
        {
            if (request.Colors < 1 || request.Colors > Palette.Length || request.Blocks < request.Colors)
                return LevelGenerationResult.Failed(0, $"Use 1 to {Palette.Length} colours and at least one block per colour.");

            var random = new Random(request.Seed);
            (int min, int max) = ScoreFor(request.Difficulty);
            LevelData closest = null;
            SolveResult closestSolution = null;
            int closestGap = int.MaxValue;
            for (int attempt = 1; attempt <= request.Attempts; attempt++)
            {
                LevelData candidate = TryBuild(request, random);
                if (candidate == null || _validator.HasErrors(_validator.Validate(candidate)))
                    continue;
                SolveResult solution = _solver.Solve(candidate);
                if (solution.Status != SolveStatus.Solvable)
                    continue;
                int score = solution.Score;
                if (score >= min && score <= max)
                    return LevelGenerationResult.Succeeded(candidate, solution, attempt, true);
                int gap = score < min ? min - score : score - max;
                if (gap < closestGap)
                {
                    closest = candidate;
                    closestSolution = solution;
                    closestGap = gap;
                }
            }

            return closest != null
                ? LevelGenerationResult.Succeeded(closest, closestSolution, request.Attempts, false)
                : LevelGenerationResult.Failed(request.Attempts,
                    "No valid, finishable level came out of these settings. Try a bigger board, fewer blocks or another seed.");
        }

        LevelData TryBuild(LevelGenerationRequest request, Random random)
        {
            var colors = new List<BlockColor>(Palette);
            Shuffle(colors, random);
            colors.RemoveRange(request.Colors, colors.Count - request.Colors);

            var doors = new List<DoorData>();
            foreach (BlockColor color in colors)
            {
                DoorData door = TryPlaceDoor(request, color, doors, random);
                if (door == null)
                    return null;
                doors.Add(door);
            }

            var taken = new HashSet<Vector2Int>();
            var blocks = new List<BlockData>();
            for (int i = 0; i < request.Blocks; i++)
            {
                DoorData door = doors[i % doors.Count];
                List<Vector2Int> cells = TryPlaceBlock(request, door, taken, random);
                if (cells == null)
                    return null;
                taken.UnionWith(cells);
                blocks.Add(new BlockData(door.Color, cells));
            }

            int timer = TimerFor(request.Blocks, request.Difficulty);
            return new LevelData(request.Width, request.Height, timer, blocks, doors, request.Difficulty);
        }

        DoorData TryPlaceDoor(LevelGenerationRequest request, BlockColor color, List<DoorData> doors, Random random)
        {
            for (int tries = 0; tries < PlacementTries; tries++)
            {
                WallSide side = Sides[random.Next(Sides.Length)];
                int wall = side is WallSide.Bottom or WallSide.Top ? request.Width : request.Height;
                int length = random.Next(1, Mathf.Min(MaxDoorLength, wall) + 1);
                int start = random.Next(0, wall - length + 1);
                var door = new DoorData(side, start, length, color);
                if (!Overlaps(door, doors))
                    return door;
            }

            return null;
        }

        bool Overlaps(DoorData door, List<DoorData> doors)
        {
            foreach (DoorData other in doors)
                if (other.Side == door.Side && other.Start <= door.End && other.End >= door.Start)
                    return true;
            return false;
        }

        /// <summary>A preset shape, turned so its span along its door's wall fits the door, on free cells.</summary>
        List<Vector2Int> TryPlaceBlock(LevelGenerationRequest request, DoorData door, HashSet<Vector2Int> taken, Random random)
        {
            bool alongX = door.Side is WallSide.Bottom or WallSide.Top;
            for (int tries = 0; tries < PlacementTries; tries++)
            {
                BlockShape shape = _shapes.Shapes[random.Next(_shapes.Shapes.Count)];
                for (int turns = random.Next(4); turns > 0; turns--)
                    shape = shape.Rotated();
                int span = alongX ? shape.Width : shape.Height;
                if (span > door.Length || shape.Width > request.Width || shape.Height > request.Height)
                    continue;
                var origin = new Vector2Int(random.Next(request.Width - shape.Width + 1),
                    random.Next(request.Height - shape.Height + 1));
                List<Vector2Int> cells = shape.At(origin);
                if (!cells.Exists(taken.Contains))
                    return cells;
            }

            return null;
        }

        void Shuffle<T>(List<T> items, Random random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }

    /// <summary>A generated level, or why none came out.</summary>
    public sealed class LevelGenerationResult
    {
        LevelGenerationResult(LevelData level, SolveResult solution, int attempts, bool matchesDifficulty, string failure)
        {
            Level = level;
            Solution = solution;
            Attempts = attempts;
            MatchesDifficulty = matchesDifficulty;
            Failure = failure;
        }

        public LevelData Level { get; }
        public SolveResult Solution { get; }
        public int Attempts { get; }

        /// <summary>False when the closest solvable level is returned because none hit the difficulty's band.</summary>
        public bool MatchesDifficulty { get; }

        public string Failure { get; }
        public bool HasLevel => Level != null;

        public static LevelGenerationResult Succeeded(LevelData level, SolveResult solution, int attempts, bool matches) =>
            new(level, solution, attempts, matches, null);

        public static LevelGenerationResult Failed(int attempts, string reason) => new(null, null, attempts, false, reason);
    }
}
