using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;
using VContainer.Unity;

namespace ColorBlockJam.Game.Home
{
    /// <summary>
    /// Entry point of the Home scene (T-24): shows the current level and the balance, plays the level, and
    /// opens the settings popup, whose switches go to <see cref="ISettingsService"/> and whose close button
    /// closes it.
    /// </summary>
    public sealed class HomeFlow : IStartable, IDisposable
    {
        readonly HomeView _view;
        readonly SettingsPopupView _settingsPopup;
        readonly IPopupService _popups;
        readonly IPlayerProgressService _playerProgress;
        readonly IWalletService _wallet;
        readonly ISettingsService _settings;
        readonly INavigationService _navigation;

        public HomeFlow(HomeView view, SettingsPopupView settingsPopup, IPopupService popups,
            IPlayerProgressService playerProgress, IWalletService wallet, ISettingsService settings,
            INavigationService navigation)
        {
            _view = view;
            _settingsPopup = settingsPopup;
            _popups = popups;
            _playerProgress = playerProgress;
            _wallet = wallet;
            _settings = settings;
            _navigation = navigation;
            _view.LevelButtonClicked += OnLevelButtonClicked;
            _view.SettingsButtonClicked += OnSettingsButtonClicked;
            _settingsPopup.CloseClicked += OnSettingsClosed;
            _settingsPopup.ToggleClicked += OnToggleClicked;
        }

        public void Start()
        {
            _view.ShowLevel(_playerProgress.CurrentLevel);
            _view.ShowCoins(_wallet.Coins);
        }

        void OnLevelButtonClicked() => _navigation.GoToGameplayAsync().Forget();

        void OnSettingsButtonClicked()
        {
            _settingsPopup.Refresh();
            _popups.Open(_settingsPopup);
        }

        void OnSettingsClosed() => _popups.Close();

        void OnToggleClicked(GameSetting setting) => _settings.Set(setting, !_settings.IsOn(setting));

        public void Dispose()
        {
            _view.LevelButtonClicked -= OnLevelButtonClicked;
            _view.SettingsButtonClicked -= OnSettingsButtonClicked;
            _settingsPopup.CloseClicked -= OnSettingsClosed;
            _settingsPopup.ToggleClicked -= OnToggleClicked;
        }
    }
}
