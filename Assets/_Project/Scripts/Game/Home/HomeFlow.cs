using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Lives;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;
using VContainer.Unity;

namespace ColorBlockJam.Game.Home
{
    public sealed class HomeFlow : IStartable, IDisposable
    {
        readonly HomeView _view;
        readonly SettingsPopupView _settingsPopup;
        readonly IPopupService _popups;
        readonly IPlayerProgressService _playerProgress;
        readonly IWalletService _wallet;
        readonly ISettingsService _settings;
        readonly INavigationService _navigation;
        readonly LivesService _lives;
        readonly LivesBarView _livesBar;
        readonly OutOfLivesPopupView _livesPopup;

        public HomeFlow(HomeView view, SettingsPopupView settingsPopup, IPopupService popups,
            IPlayerProgressService playerProgress, IWalletService wallet, ISettingsService settings,
            INavigationService navigation, LivesService lives, LivesBarView livesBar, OutOfLivesPopupView livesPopup)
        {
            _lives = lives;
            _livesBar = livesBar;
            _livesPopup = livesPopup;
            _livesBar.Clicked += OpenLivesPopup;
            _livesPopup.RefillClicked += OnRefillClicked;
            _livesPopup.CloseClicked += OnLivesPopupClosed;
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
            _lives.Refresh();
            _livesBar.Show(Lives());
        }

        void OnLevelButtonClicked()
        {
            if (_lives.Count == 0)
            {
                OpenLivesPopup();
                return;
            }

            _navigation.GoToGameplayAsync().Forget();
        }

        void OpenLivesPopup()
        {
            _livesPopup.SetOffer(Lives(), _lives.RefillPrice, _wallet.Coins >= _lives.RefillPrice);
            _popups.Open(_livesPopup);
        }

        void OnRefillClicked()
        {
            if (_lives.TryRefill())
                _popups.Close();
        }

        void OnLivesPopupClosed() => _popups.Close();

        LivesChangedSignal Lives() => new(_lives.Count, _lives.Max, _lives.SecondsToNextLife());

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
            _livesBar.Clicked -= OpenLivesPopup;
            _livesPopup.RefillClicked -= OnRefillClicked;
            _livesPopup.CloseClicked -= OnLivesPopupClosed;
        }
    }
}
