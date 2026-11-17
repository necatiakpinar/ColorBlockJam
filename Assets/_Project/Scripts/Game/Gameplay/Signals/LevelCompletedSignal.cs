namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>The attempt is won: every block left the board. Fired once per attempt.</summary>
    public readonly struct LevelCompletedSignal
    {
        public readonly int LevelNumber;

        public LevelCompletedSignal(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }
}
