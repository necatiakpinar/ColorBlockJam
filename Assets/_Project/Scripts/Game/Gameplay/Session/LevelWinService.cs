using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;

namespace ColorBlockJam.Game.Gameplay.Session
{
    public sealed class LevelWinService : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly LevelProvider _levels;
        readonly LevelRewardProvider _rewards;
        readonly IPlayerProgressService _progress;
        readonly IWalletService _wallet;
        readonly IKeyValueStorage _storage;

        public LevelWinService(SignalBus signalBus, LevelProvider levels, LevelRewardProvider rewards,
            IPlayerProgressService progress, IWalletService wallet, IKeyValueStorage storage)
        {
            _signalBus = signalBus;
            _levels = levels;
            _rewards = rewards;
            _progress = progress;
            _wallet = wallet;
            _storage = storage;
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }

        void OnLevelCompleted(LevelCompletedSignal signal)
        {
            int coins = _rewards.CoinsFor(_levels.GetLevel(signal.LevelNumber));
            int before = _wallet.Coins;
            using (_storage.DeferFlush())
            {
                _progress.AdvanceLevel();
                _wallet.Add(coins);
            }

            _signalBus.Fire(new LevelRewardedSignal(signal.LevelNumber, coins, before));
        }

        public void Dispose() => _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
    }
}
