using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;

namespace ColorBlockJam.Game.Gameplay
{
    public sealed class FailFlow : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IPopupService _popups;
        readonly OutOfTimePopupView _outOfTime;
        readonly LevelFailedPopupView _levelFailed;
        readonly LevelSession _session;
        readonly LevelContinueService _continues;
        readonly INavigationService _navigation;
        readonly LevelAttemptLives _attempt;
        readonly GameplayLivesFlow _lives;

        public FailFlow(SignalBus signalBus, IPopupService popups, OutOfTimePopupView outOfTime,
            LevelFailedPopupView levelFailed, LevelSession session, LevelContinueService continues,
            INavigationService navigation, LevelAttemptLives attempt, GameplayLivesFlow lives)
        {
            _attempt = attempt;
            _lives = lives;
            _signalBus = signalBus;
            _popups = popups;
            _outOfTime = outOfTime;
            _levelFailed = levelFailed;
            _session = session;
            _continues = continues;
            _navigation = navigation;
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
            _outOfTime.ContinueClicked += OnContinueClicked;
            _outOfTime.CloseClicked += OnGiveUp;
            _levelFailed.RetryClicked += OnRetryClicked;
            _levelFailed.CloseClicked += OnHomeClicked;
        }

        void OnLevelFailed(LevelFailedSignal signal)
        {
            _outOfTime.SetOffer(_continues.ExtraSeconds, _continues.Cost, _continues.CanAfford);
            _popups.Open(_outOfTime);
        }

        void OnContinueClicked()
        {
            _popups.Close();
            if (!_continues.TryContinue())
                _popups.Open(_levelFailed);
        }

        void OnGiveUp()
        {
            _attempt.Abandon();
            _popups.Close();
            _popups.Open(_levelFailed);
        }

        void OnRetryClicked()
        {
            _popups.Close();
            _lives.PlayAgain();
        }

        void OnHomeClicked() => _navigation.GoToHomeAsync().Forget();

        public void Dispose()
        {
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
            _outOfTime.ContinueClicked -= OnContinueClicked;
            _outOfTime.CloseClicked -= OnGiveUp;
            _levelFailed.RetryClicked -= OnRetryClicked;
            _levelFailed.CloseClicked -= OnHomeClicked;
        }
    }
}
