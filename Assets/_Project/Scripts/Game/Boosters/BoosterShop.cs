using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Boosters
{
    public sealed class BoosterShop
    {
        readonly IWalletService _wallet;
        readonly BoosterInventory _inventory;
        readonly BoosterConfig _config;
        readonly IKeyValueStorage _storage;

        public BoosterShop(IWalletService wallet, BoosterInventory inventory, BoosterConfig config, IKeyValueStorage storage)
        {
            _wallet = wallet;
            _inventory = inventory;
            _config = config;
            _storage = storage;
        }

        public int PriceOf(BoosterType type) => _config.For(type).Price;

        public bool CanAfford(BoosterType type) => _wallet.Coins >= PriceOf(type);

        public bool TryBuy(BoosterType type)
        {
            using (_storage.DeferFlush())
            {
                if (!_wallet.TrySpend(PriceOf(type)))
                    return false;
                _inventory.Add(type, 1);
                return true;
            }
        }
    }
}
