using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Persistence;
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
    /// <see cref="LevelSession"/>, and routes the HUD and the pause popup. Restart restarts at once; pause
    /// pauses the session, then opens the pause popup; closing it resumes; its switches go to
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

        public GameplayFlow(IPlayerProgressService playerProgress, LevelSession session, GameplayHudView hudView,
            PausePopupView pausePopup, IPopupService popups, ISettingsService settings, INavigationService navigation)
        {
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

        void OnRestartClicked() => _session.Restart();

        void OnPauseClicked()
        {
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

        void OnHomeClicked() => _navigation.GoToHomeAsync().Forget();

        public void Dispose()
        {
            _hudView.RestartClicked -= OnRestartClicked;
            _hudView.PauseClicked -= OnPauseClicked;
            _pausePopup.CloseClicked -= OnPauseClosed;
            _pausePopup.HomeClicked -= OnHomeClicked;
            _pausePopup.ToggleClicked -= OnToggleClicked;
        }
    }
}
