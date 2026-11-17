namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct TimerChangedSignal
    {
        public readonly int SecondsLeft;

        public TimerChangedSignal(int secondsLeft)
        {
            SecondsLeft = secondsLeft;
        }
    }
}
