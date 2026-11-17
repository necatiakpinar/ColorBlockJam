namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// The level timer shows a new whole second: fired when a level starts and each time the shown value
    /// drops, never twice for the same value in a row.
    /// </summary>
    public readonly struct TimerChangedSignal
    {
        /// <summary>Seconds left, rounded up (a timer at 0.4 s shows 1).</summary>
        public readonly int SecondsLeft;

        public TimerChangedSignal(int secondsLeft)
        {
            SecondsLeft = secondsLeft;
        }
    }
}
