namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct LevelFailedSignal
    {
        public readonly int LevelNumber;

        public LevelFailedSignal(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }
}
