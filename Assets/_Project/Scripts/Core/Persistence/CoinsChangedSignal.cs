namespace ColorBlockJam.Core.Persistence
{
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
