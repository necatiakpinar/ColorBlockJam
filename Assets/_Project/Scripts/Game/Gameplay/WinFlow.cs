using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;

namespace ColorBlockJam.Game.Gameplay
{
    public sealed class WinFlow : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IPopupService _popups;
        readonly WinPopupView _winPopup;
        readonly GameplayConfig _config;
        readonly IGameClock _clock;
        readonly ICoinFlight _coinFlight;
        readonly INavigationService _navigation;
        readonly CancellationTokenSource _lifetime = new();
        int _reward;
        bool _leaving;

        public WinFlow(SignalBus signalBus, IPopupService popups, WinPopupView winPopup, GameplayConfig config,
            IGameClock clock, ICoinFlight coinFlight, INavigationService navigation)
        {
            _signalBus = signalBus;
            _popups = popups;
            _winPopup = winPopup;
            _config = config;
            _clock = clock;
            _coinFlight = coinFlight;
            _navigation = navigation;
            _signalBus.Subscribe<LevelRewardedSignal>(OnLevelRewarded);
            _winPopup.ContinueClicked += OnContinueClicked;
        }

        void OnLevelRewarded(LevelRewardedSignal signal)
        {
            _reward = signal.Coins;
            _coinFlight.HoldCounter(signal.BalanceBefore);
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
            _lifetime.Cancel();
            _signalBus.Unsubscribe<LevelRewardedSignal>(OnLevelRewarded);
            _winPopup.ContinueClicked -= OnContinueClicked;
        }
    }
}
