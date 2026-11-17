namespace ColorBlockJam.Game.Lives
{
    public readonly struct LivesChangedSignal
    {
        public readonly int Count;
        public readonly int Max;
        public readonly int SecondsToNextLife;

        public LivesChangedSignal(int count, int max, int secondsToNextLife)
        {
            Count = count;
            Max = max;
            SecondsToNextLife = secondsToNextLife;
        }

        public bool IsFull => Count >= Max;
    }
}
