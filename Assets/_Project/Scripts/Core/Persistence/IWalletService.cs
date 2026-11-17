namespace ColorBlockJam.Core.Persistence
{
    public interface IWalletService
    {
        int Coins { get; }

        void Load();

        void Add(int amount);

        bool TrySpend(int amount);
    }
}
