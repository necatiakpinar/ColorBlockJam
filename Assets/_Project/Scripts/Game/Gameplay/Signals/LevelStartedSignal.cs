namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct LevelStartedSignal
    {
        public readonly int LevelNumber;

        public LevelStartedSignal(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }
}
