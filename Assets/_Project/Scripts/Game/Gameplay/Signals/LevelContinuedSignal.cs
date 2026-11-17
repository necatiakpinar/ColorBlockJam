namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>A lost level plays on with the board as it was and <see cref="ExtraSeconds"/> on the timer.</summary>
    public readonly struct LevelContinuedSignal
    {
        public readonly int ExtraSeconds;

        public LevelContinuedSignal(int extraSeconds)
        {
            ExtraSeconds = extraSeconds;
        }
    }
}
