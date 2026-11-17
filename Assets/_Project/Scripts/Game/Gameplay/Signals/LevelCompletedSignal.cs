namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct LevelCompletedSignal
    {
        public readonly int LevelNumber;

        public LevelCompletedSignal(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }
}
