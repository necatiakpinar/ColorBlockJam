namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct LevelContinuedSignal
    {
        public readonly int ExtraSeconds;

        public LevelContinuedSignal(int extraSeconds)
        {
            ExtraSeconds = extraSeconds;
        }
    }
}
