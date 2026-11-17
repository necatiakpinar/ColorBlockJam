namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>The attempt is lost (the timer ran out). Fired once per attempt.</summary>
    public readonly struct LevelFailedSignal
    {
        public readonly int LevelNumber;

        public LevelFailedSignal(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }
}
