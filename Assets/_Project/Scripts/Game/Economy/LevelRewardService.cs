using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;

namespace ColorBlockJam.Game.Economy
{
    /// <summary>
    /// Pays a win into the wallet the moment the level is won, before anything presents it (T-16, DA-7). The
    /// session announces a win once per attempt, so a win pays once; failing, restarting or leaving never
    /// touches the wallet.
    /// </summary>
    public sealed class LevelRewardService : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly LevelProvider _levels;
        readonly LevelRewardProvider _rewards;
        readonly IWalletService _wallet;

        public LevelRewardService(SignalBus signalBus, LevelProvider levels, LevelRewardProvider rewards, IWalletService wallet)
        {
            _signalBus = signalBus;
            _levels = levels;
            _rewards = rewards;
            _wallet = wallet;
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }

        void OnLevelCompleted(LevelCompletedSignal signal) =>
            _wallet.Add(_rewards.CoinsFor(_levels.GetLevel(signal.LevelNumber)));

        public void Dispose() => _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
    }
}
