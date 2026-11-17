using System;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Lives
{
    public sealed class LivesService
    {
        public const string StorageKey = "lives_v1";

        readonly JsonRecord<LivesData> _record;
        readonly LivesConfig _config;
        readonly ITrustedClock _clock;
        readonly IWalletService _wallet;
        readonly SignalBus _signalBus;
        LivesData _data;

        public LivesService(IKeyValueStorage storage, IGameLogger logger, LivesConfig config, ITrustedClock clock,
            IWalletService wallet, SignalBus signalBus)
        {
            _record = new JsonRecord<LivesData>(StorageKey, storage, logger);
            _config = config;
            _clock = clock;
            _wallet = wallet;
            _signalBus = signalBus;
        }

        public int Max => _config.MaxLives;
        public int Count => Max - Data.Missing;
        public bool IsFull => Data.Missing == 0;
        public int RefillPrice => _config.RefillPrice;

        LivesData Data => _data ?? throw new InvalidOperationException($"{nameof(LivesService)} read before Load().");

        public void Load() => _data = _record.LoadOrCreate(data => data.Missing >= 0 && data.Missing <= Max);

        public void Refresh()
        {
            if (IsFull || !_clock.TryGetUtcNow(out DateTime now))
                return;
            if (Data.NextLifeUtcTicks == 0)
            {
                Data.NextLifeUtcTicks = now.Ticks + RefillTicks;
                Save();
                return;
            }

            int before = Data.Missing;
            while (Data.Missing > 0 && now.Ticks >= Data.NextLifeUtcTicks)
            {
                Data.Missing--;
                Data.NextLifeUtcTicks = Data.Missing > 0 ? Data.NextLifeUtcTicks + RefillTicks : 0;
            }

            if (Data.Missing != before)
                Change();
        }

        public int SecondsToNextLife()
        {
            if (IsFull || Data.NextLifeUtcTicks == 0 || !_clock.TryGetUtcNow(out DateTime now))
                return -1;
            long left = Math.Max(0, Data.NextLifeUtcTicks - now.Ticks);
            return (int)Math.Ceiling(left / (double)TimeSpan.TicksPerSecond);
        }

        public bool TryLoseOne()
        {
            if (Count == 0)
                return false;
            Data.Missing++;
            if (Data.NextLifeUtcTicks == 0 && _clock.TryGetUtcNow(out DateTime now))
                Data.NextLifeUtcTicks = now.Ticks + RefillTicks;
            Change();
            return true;
        }

        public bool TryRefill()
        {
            if (IsFull || !_wallet.TrySpend(RefillPrice))
                return false;
            Data.Missing = 0;
            Data.NextLifeUtcTicks = 0;
            Change();
            return true;
        }

        public void Announce() => _signalBus.Fire(new LivesChangedSignal(Count, Max, SecondsToNextLife()));

        long RefillTicks => _config.RefillSeconds * TimeSpan.TicksPerSecond;

        void Change()
        {
            Save();
            Announce();
        }

        void Save() => _record.Save(_data);
    }
}
