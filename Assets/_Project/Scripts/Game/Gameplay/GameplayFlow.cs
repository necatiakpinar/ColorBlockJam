using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Gameplay.Boosters;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;
using VContainer.Unity;

namespace ColorBlockJam.Game.Gameplay
{
    /// <summary>
    /// Entry point of the gameplay scene: starts the attempt at the current level through
    /// <see cref="LevelSession"/>, and routes the HUD and the pause popup. Restart and Home cost a started
    /// attempt's life (Home asks first in the leave popup) (<see cref="LevelAttemptLives"/>, DA-38); restart plays again through
    /// <see cref="GameplayLivesFlow"/>, which needs a life; pause
    /// pauses the session (putting back a booster that waits for a target), then opens the pause popup; closing
    /// it resumes; its switches go to
    /// <see cref="ISettingsService"/>; Home leaves. The session and the settings own every change.
    /// </summary>
    public sealed class GameplayFlow : IStartable, IDisposable
    {
        readonly IPlayerProgressService _playerProgress;
        readonly LevelSession _session;
        readonly GameplayHudView _hudView;
        readonly PausePopupView _pausePopup;
        readonly IPopupService _popups;
        readonly ISettingsService _settings;
        readonly INavigationService _navigation;
        readonly BoosterService _boosters;
        readonly LevelAttemptLives _attempt;
        readonly GameplayLivesFlow _lives;
        readonly LeaveLevelPopupView _leavePopup;

        public GameplayFlow(IPlayerProgressService playerProgress, LevelSession session, GameplayHudView hudView,
            PausePopupView pausePopup, IPopupService popups, ISettingsService settings, INavigationService navigation,
            BoosterService boosters, LevelAttemptLives attempt, GameplayLivesFlow lives, LeaveLevelPopupView leavePopup)
        {
            _leavePopup = leavePopup;
            _leavePopup.LeaveClicked += OnLeaveConfirmed;
            _leavePopup.CloseClicked += OnLeaveCancelled;
            _attempt = attempt;
            _lives = lives;
            _boosters = boosters;
            _playerProgress = playerProgress;
            _session = session;
            _hudView = hudView;
            _pausePopup = pausePopup;
            _popups = popups;
            _settings = settings;
            _navigation = navigation;
            _hudView.RestartClicked += OnRestartClicked;
            _hudView.PauseClicked += OnPauseClicked;
            _pausePopup.CloseClicked += OnPauseClosed;
            _pausePopup.HomeClicked += OnHomeClicked;
            _pausePopup.ToggleClicked += OnToggleClicked;
        }

        public void Start() => _session.Start(_playerProgress.CurrentLevel);

        /// <summary>A started attempt costs its life (DA-38); playing again needs one.</summary>
        void OnRestartClicked()
        {
            _boosters.CancelTargeting();
            _attempt.Abandon();
            _lives.PlayAgain();
        }

        void OnPauseClicked()
        {
            _boosters.CancelTargeting();
            if (!_session.Pause())
                return;
            _pausePopup.Refresh();
            _popups.Open(_pausePopup);
        }

        void OnPauseClosed()
        {
            _popups.Close();
            _session.Resume();
        }

        void OnToggleClicked(GameSetting setting) => _settings.Set(setting, !_settings.IsOn(setting));

        /// <summary>Home from the pause popup; when that would cost a life, ask first (DA-38).</summary>
        void OnHomeClicked()
        {
            if (!_attempt.WouldCostLife)
            {
                _navigation.GoToHomeAsync().Forget();
                return;
            }

            _popups.Close();
            _popups.Open(_leavePopup);
        }

        void OnLeaveConfirmed()
        {
            _attempt.Abandon();
            _navigation.GoToHomeAsync().Forget();
        }

        /// <summary>Staying goes back to the pause popup; the level is still paused.</summary>
        void OnLeaveCancelled()
        {
            _popups.Close();
            _pausePopup.Refresh();
            _popups.Open(_pausePopup);
        }

        public void Dispose()
        {
            _hudView.RestartClicked -= OnRestartClicked;
            _hudView.PauseClicked -= OnPauseClicked;
            _pausePopup.CloseClicked -= OnPauseClosed;
            _pausePopup.HomeClicked -= OnHomeClicked;
            _pausePopup.ToggleClicked -= OnToggleClicked;
            _leavePopup.LeaveClicked -= OnLeaveConfirmed;
            _leavePopup.CloseClicked -= OnLeaveCancelled;
        }
    }
}
