namespace ColorBlockJam.Core.Persistence
{
    /// <summary>The player's coins (persistence.md). Every change is saved at once and announced with
    /// <see cref="CoinsChangedSignal"/>.</summary>
    public interface IWalletService
    {
        /// <summary>Coins the player has. Throws before <see cref="Load"/>.</summary>
        int Coins { get; }

        void Load();

        /// <summary>Adds a positive number of coins; zero or less is a bug and throws.</summary>
        void Add(int amount);
    }
}
