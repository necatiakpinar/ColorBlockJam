using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;

namespace ColorBlockJam.Game.Gameplay
{
    public sealed class FailFlow : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IPopupService _popups;
        readonly LevelFailedPopupView _levelFailed;
        readonly INavigationService _navigation;
        readonly GameplayLivesFlow _lives;

        public FailFlow(SignalBus signalBus, IPopupService popups, LevelFailedPopupView levelFailed,
            INavigationService navigation, GameplayLivesFlow lives)
        {
            _signalBus = signalBus;
            _popups = popups;
            _levelFailed = levelFailed;
            _navigation = navigation;
            _lives = lives;
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
            _levelFailed.RetryClicked += OnRetryClicked;
            _levelFailed.HomeClicked += OnHomeClicked;
        }

        void OnLevelFailed(LevelFailedSignal signal) => _popups.Open(_levelFailed);

        void OnRetryClicked()
        {
            _popups.Close();
            _lives.PlayAgain();
        }

        void OnHomeClicked() => _navigation.GoToHomeAsync().Forget();

        public void Dispose()
        {
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
            _levelFailed.RetryClicked -= OnRetryClicked;
            _levelFailed.HomeClicked -= OnHomeClicked;
        }
    }
}
