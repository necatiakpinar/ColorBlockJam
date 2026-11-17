using System;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Boosters
{
    /// <summary>
    /// How many of each booster the player owns (boosters.md). A booster missing from the save (a new player, or
    /// a booster added later) starts at its <see cref="BoosterSettings.StartingCount"/>. Every change is saved at
    /// once and announced with <see cref="BoosterCountChangedSignal"/>.
    /// </summary>
    public sealed class BoosterInventory
    {
        public const string StorageKey = "boosters_v1";

        readonly JsonRecord<BoosterInventoryData> _record;
        readonly BoosterConfig _config;
        readonly SignalBus _signalBus;
        BoosterInventoryData _data;

        public BoosterInventory(IKeyValueStorage storage, IGameLogger logger, BoosterConfig config, SignalBus signalBus)
        {
            _record = new JsonRecord<BoosterInventoryData>(StorageKey, storage, logger);
            _config = config;
            _signalBus = signalBus;
        }

        BoosterInventoryData Data =>
            _data ?? throw new InvalidOperationException($"{nameof(BoosterInventory)} read before Load().");

        public void Load()
        {
            _data = _record.LoadOrCreate(IsValid);
            bool added = false;
            foreach (BoosterSettings settings in _config.Boosters)
            {
                if (Find(settings.Type) != null)
                    continue;
                _data.Stocks.Add(new BoosterStock(settings.Type, settings.StartingCount));
                added = true;
            }

            if (added)
                _record.Save(_data);
        }

        public int Count(BoosterType type) => Stock(type).Count;

        /// <summary>Adds a positive number of one booster; zero or less is a bug and throws.</summary>
        public void Add(BoosterType type, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Only a positive number of boosters can be added.");
            Change(Stock(type), amount);
        }

        /// <summary>Takes one booster if the player has one; false (and no change) if not.</summary>
        public bool TryUse(BoosterType type)
        {
            BoosterStock stock = Stock(type);
            if (stock.Count <= 0)
                return false;
            Change(stock, -1);
            return true;
        }

        void Change(BoosterStock stock, int delta)
        {
            stock.Count += delta;
            _record.Save(_data);
            _signalBus.Fire(new BoosterCountChangedSignal(stock.Type, stock.Count));
        }

        BoosterStock Stock(BoosterType type) =>
            Find(type) ?? throw new InvalidOperationException($"No stock for the {type} booster; is it in the BoosterConfig?");

        BoosterStock Find(BoosterType type)
        {
            foreach (BoosterStock stock in Data.Stocks)
                if (stock.Type == type)
                    return stock;
            return null;
        }

        bool IsValid(BoosterInventoryData data)
        {
            foreach (BoosterStock stock in data.Stocks)
                if (stock.Count < 0)
                    return false;
            return true;
        }
    }
}
