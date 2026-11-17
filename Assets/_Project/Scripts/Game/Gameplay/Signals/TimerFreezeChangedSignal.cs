namespace ColorBlockJam.Game.Gameplay.Signals
{
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
