namespace ColorBlockJam.Game.Levels.Generation
{
    /// <summary>What the level generator is asked to build (T-47, level-editor.md).</summary>
    public readonly struct LevelGenerationRequest
    {
        public LevelGenerationRequest(int width, int height, int colors, int blocks, LevelDifficulty difficulty, int seed,
            int attempts)
        {
            Width = width;
            Height = height;
            Colors = colors;
            Blocks = blocks;
            Difficulty = difficulty;
            Seed = seed;
            Attempts = attempts;
        }

        public int Width { get; }
        public int Height { get; }

        /// <summary>How many block colours (and so doors) the level uses; every colour gets at least one block.</summary>
        public int Colors { get; }

        public int Blocks { get; }
        public LevelDifficulty Difficulty { get; }

        /// <summary>The same seed with the same settings gives the same level.</summary>
        public int Seed { get; }

        /// <summary>Candidates to try before giving up.</summary>
        public int Attempts { get; }
    }
}
