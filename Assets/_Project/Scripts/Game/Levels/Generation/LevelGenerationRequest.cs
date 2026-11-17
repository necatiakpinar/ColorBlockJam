namespace ColorBlockJam.Game.Levels.Generation
{
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

        public int Colors { get; }

        public int Blocks { get; }
        public LevelDifficulty Difficulty { get; }

        public int Seed { get; }

        public int Attempts { get; }
    }
}
