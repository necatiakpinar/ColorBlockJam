namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct LevelRewardedSignal
    {
        public readonly int LevelNumber;
        public readonly int Coins;
        public readonly int BalanceBefore;

        public LevelRewardedSignal(int levelNumber, int coins, int balanceBefore)
        {
            LevelNumber = levelNumber;
            Coins = coins;
            BalanceBefore = balanceBefore;
        }
    }
}
