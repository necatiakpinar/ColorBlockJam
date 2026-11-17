using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.UI;

namespace ColorBlockJam.Game.Gameplay
{
    /// <summary>
    /// Presents a win (T-21): <see cref="GameplayConfig.WinPopupDelay"/> after <see cref="LevelCompletedSignal"/>,
    /// so the last block is seen leaving, it opens the win popup with the reward the win already paid;
    /// Continue starts the next level. By then <see cref="LevelProgression"/> has advanced the level number.
    /// </summary>
    public sealed class WinFlow : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IPopupService _popups;
        readonly WinPopupView _winPopup;
        readonly LevelSession _session;
        readonly IPlayerProgressService _progress;
        readonly LevelProvider _levels;
        readonly LevelRewardProvider _rewards;
        readonly GameplayConfig _config;
        readonly IGameClock _clock;
        readonly CancellationTokenSource _lifetime = new();

        public WinFlow(SignalBus signalBus, IPopupService popups, WinPopupView winPopup, LevelSession session,
            IPlayerProgressService progress, LevelProvider levels, LevelRewardProvider rewards, GameplayConfig config,
            IGameClock clock)
        {
            _signalBus = signalBus;
            _popups = popups;
            _winPopup = winPopup;
            _session = session;
            _progress = progress;
            _levels = levels;
            _rewards = rewards;
            _config = config;
            _clock = clock;
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _winPopup.ContinueClicked += OnContinueClicked;
        }

        void OnLevelCompleted(LevelCompletedSignal signal) =>
            OpenAfterDelayAsync(_rewards.CoinsFor(_levels.GetLevel(signal.LevelNumber)), _lifetime.Token).Forget();

        async UniTaskVoid OpenAfterDelayAsync(int reward, CancellationToken cancellation)
        {
            if (await _clock.DelayAsync(_config.WinPopupDelay, cancellation).SuppressCancellationThrow())
                return;
            _winPopup.SetReward(reward);
            _popups.Open(_winPopup);
        }

        void OnContinueClicked()
        {
            _popups.Close();
            _session.Next(_progress.CurrentLevel);
        }

        public void Dispose()
        {
            // Cancelled, not disposed: a second Dispose stays harmless.
            _lifetime.Cancel();
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _winPopup.ContinueClicked -= OnContinueClicked;
        }
    }
}
