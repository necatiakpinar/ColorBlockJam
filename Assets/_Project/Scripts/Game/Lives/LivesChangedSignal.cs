namespace ColorBlockJam.Game.Lives
{
    /// <summary>
    /// The lives changed, or a second of a refill countdown passed. <see cref="SecondsToNextLife"/> is -1 when the
    /// lives are full or the trusted time is not known yet.
    /// </summary>
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
