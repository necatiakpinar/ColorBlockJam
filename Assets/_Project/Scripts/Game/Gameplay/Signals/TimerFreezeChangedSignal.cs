namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// The freeze booster's state changed (DA-36): it started, stopped or resumed counting with the level, or ended
    /// (<see cref="SecondsLeft"/> is 0). While <see cref="Counting"/>, the freeze runs out in
    /// <see cref="SecondsLeft"/> seconds of real time.
    /// </summary>
    public readonly struct TimerFreezeChangedSignal
    {
        public readonly float SecondsLeft;
        public readonly float TotalSeconds;
        public readonly bool Counting;

        public TimerFreezeChangedSignal(float secondsLeft, float totalSeconds, bool counting)
        {
            SecondsLeft = secondsLeft;
            TotalSeconds = totalSeconds;
            Counting = counting;
        }

        public bool IsFrozen => SecondsLeft > 0f;
    }
}
