using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Boosters
{
    public sealed class BoosterShop
    {
        readonly IWalletService _wallet;
        readonly BoosterInventory _inventory;
        readonly BoosterConfig _config;

        public BoosterShop(IWalletService wallet, BoosterInventory inventory, BoosterConfig config)
        {
            _wallet = wallet;
            _inventory = inventory;
            _config = config;
        }

        public int PriceOf(BoosterType type) => _config.For(type).Price;

        public bool CanAfford(BoosterType type) => _wallet.Coins >= PriceOf(type);

        public bool TryBuy(BoosterType type)
        {
            if (!_wallet.TrySpend(PriceOf(type)))
                return false;
            _inventory.Add(type, 1);
            return true;
        }
    }
}
