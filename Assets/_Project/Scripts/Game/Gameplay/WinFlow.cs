using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;

namespace ColorBlockJam.Game.Gameplay
{
    /// <summary>
    /// Presents a win (T-21, T-51, DA-12). At the win the HUD counter keeps the balance from before the reward, which
    /// the wallet has already paid. <see cref="GameplayConfig.WinPopupDelay"/> later, once the last block is seen
    /// leaving, the win popup shows the reward. Continue closes it, flies the coins from its pile to the counter,
    /// counting them in, and goes Home, where the next level waits. <see cref="LevelProgression"/> has
    /// already advanced the level number.
    /// </summary>
    public sealed class WinFlow : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IPopupService _popups;
        readonly WinPopupView _winPopup;
        readonly LevelProvider _levels;
        readonly LevelRewardProvider _rewards;
        readonly GameplayConfig _config;
        readonly IGameClock _clock;
        readonly IWalletService _wallet;
        readonly ICoinFlight _coinFlight;
        readonly INavigationService _navigation;
        readonly CancellationTokenSource _lifetime = new();
        int _reward;
        bool _leaving;

        public WinFlow(SignalBus signalBus, IPopupService popups, WinPopupView winPopup, LevelProvider levels,
            LevelRewardProvider rewards, GameplayConfig config, IGameClock clock, IWalletService wallet,
            ICoinFlight coinFlight, INavigationService navigation)
        {
            _signalBus = signalBus;
            _popups = popups;
            _winPopup = winPopup;
            _levels = levels;
            _rewards = rewards;
            _config = config;
            _clock = clock;
            _wallet = wallet;
            _coinFlight = coinFlight;
            _navigation = navigation;
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _winPopup.ContinueClicked += OnContinueClicked;
        }

        void OnLevelCompleted(LevelCompletedSignal signal)
        {
            _reward = _rewards.CoinsFor(_levels.GetLevel(signal.LevelNumber));
            // The reward is paid within this same signal, before or after this line; either way the frame shows this.
            _coinFlight.HoldCounter(_wallet.Coins - _reward);
            OpenAfterDelayAsync(_lifetime.Token).Forget();
        }

        async UniTaskVoid OpenAfterDelayAsync(CancellationToken cancellation)
        {
            if (await _clock.DelayAsync(_config.WinPopupDelay, cancellation).SuppressCancellationThrow())
                return;
            _winPopup.SetReward(_reward);
            _popups.Open(_winPopup);
        }

        void OnContinueClicked()
        {
            if (_leaving)
                return;
            _leaving = true;
            LeaveAsync(_lifetime.Token).Forget();
        }

        async UniTaskVoid LeaveAsync(CancellationToken cancellation)
        {
            _popups.Close();
            if (await _coinFlight.FlyAsync(_winPopup.CoinPile, _reward, cancellation).SuppressCancellationThrow())
                return;
            if (await _clock.DelayAsync(_config.CoinFlight.HomeDelay, cancellation).SuppressCancellationThrow())
                return;
            await _navigation.GoToHomeAsync();
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
