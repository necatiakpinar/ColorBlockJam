using System;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Lives
{
    /// <summary>
    /// The player's lives (lives.md, DA-38). A missing life comes back after <see cref="LivesConfig.RefillSeconds"/>
    /// of trusted time, one after the other, never above <see cref="LivesConfig.MaxLives"/>; the countdown reads
    /// <see cref="ITrustedClock"/>, so moving the device clock changes nothing (R-44). Coins fill every missing
    /// life at once. Every change is saved at once and announced with <see cref="LivesChangedSignal"/>.
    /// </summary>
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

        /// <summary>
        /// Gives back every life whose countdown has run out, and starts the countdown of a missing life when none
        /// runs yet (a life lost before the trusted time was known). Does nothing until that time is known.
        /// </summary>
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

        /// <summary>Whole seconds until the next life comes back; -1 when full or the time is not known yet.</summary>
        public int SecondsToNextLife()
        {
            if (IsFull || Data.NextLifeUtcTicks == 0 || !_clock.TryGetUtcNow(out DateTime now))
                return -1;
            long left = Math.Max(0, Data.NextLifeUtcTicks - now.Ticks);
            return (int)Math.Ceiling(left / (double)TimeSpan.TicksPerSecond);
        }

        /// <summary>Takes one life; false (and no change) when none are left.</summary>
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

        /// <summary>Fills every missing life for <see cref="RefillPrice"/> coins; false when full or short of coins.</summary>
        public bool TryRefill()
        {
            if (IsFull || !_wallet.TrySpend(RefillPrice))
                return false;
            Data.Missing = 0;
            Data.NextLifeUtcTicks = 0;
            Change();
            return true;
        }

        /// <summary>Tells the lives views where things stand (a countdown second, or after a change).</summary>
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
