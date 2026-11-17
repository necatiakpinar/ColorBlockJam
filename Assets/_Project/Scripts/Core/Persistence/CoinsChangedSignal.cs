namespace ColorBlockJam.Core.Persistence
{
    /// <summary>The wallet changed and is already saved.</summary>
    public readonly struct CoinsChangedSignal
    {
        public readonly int Coins;
        public readonly int Delta;

        public CoinsChangedSignal(int coins, int delta)
        {
            Coins = coins;
            Delta = delta;
        }
    }
}
