using System;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Signals;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class WalletService : IWalletService
    {
        public const string StorageKey = "wallet_v1";

        readonly JsonRecord<WalletData> _record;
        readonly SignalBus _signalBus;
        WalletData _data;

        public WalletService(IKeyValueStorage storage, IGameLogger logger, SignalBus signalBus)
        {
            _record = new JsonRecord<WalletData>(StorageKey, storage, logger);
            _signalBus = signalBus;
        }

        public int Coins => Data.Coins;

        WalletData Data => _data ?? throw new InvalidOperationException($"{nameof(WalletService)} read before Load().");

        public void Load() => _data = _record.LoadOrCreate(data => data.Coins >= 0);

        public void Add(int amount)
        {
            RequirePositive(amount);
            Change(amount);
        }

        public bool TrySpend(int amount)
        {
            RequirePositive(amount);
            if (Data.Coins < amount)
                return false;
            Change(-amount);
            return true;
        }

        void RequirePositive(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Only a positive number of coins can change hands.");
        }

        void Change(int delta)
        {
            Data.Coins += delta;
            _record.Save(_data);
            _signalBus.Fire(new CoinsChangedSignal(_data.Coins, delta));
        }
    }
}
