using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.UI;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine.UI;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class HomeFlowTests
    {
        TestViews _views;
        RecordingNavigation _navigation;
        RecordingPopupService _popups;
        SignalBus _bus;
        PlayerProgressService _progress;
        WalletService _wallet;
        SettingsService _settings;
        HomeView _view;
        Button _levelButton;
        Button _settingsButton;
        SettingsPopupView _settingsPopup;
        Button _closeButton;
        SettingToggleView[] _toggles;
        LivesRig _lives;
        LivesBarView _livesBar;
        OutOfLivesPopupView _livesPopup;
        Button _plusButton;
        Button _refillButton;
        Button _livesCloseButton;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _navigation = new RecordingNavigation();
            _popups = new RecordingPopupService();
            _bus = new SignalBus();
            _progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            _progress.Load();
            _wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _wallet.Load();
            _settings = new SettingsService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _settings.Load();
            _view = _views.CreateHomeView(out _levelButton, out _settingsButton);
            _settingsPopup = _views.CreateSettingsPopup(_bus, _settings, out _closeButton, out _toggles);
            _lives = new LivesRig(_bus, _wallet);
            _livesBar = _views.CreateLivesBar(_bus, out _plusButton);
            _livesPopup = _views.CreateOutOfLivesPopup(_bus, out _refillButton, out _livesCloseButton);
        }

        [TearDown]
        public void TearDown()
        {
            _views.Dispose();
            _lives.Dispose();
        }

        HomeFlow CreateFlow() =>
            new(_view, _settingsPopup, _popups, _progress, _wallet, _settings, _navigation, _lives.Lives, _livesBar,
                _livesPopup);

        [Test]
        public void Start_ShouldShowFullLives_WhenNoLifeIsMissing()
        {
            using HomeFlow flow = CreateFlow();

            flow.Start();

            Assert.That(_livesBar.CountText, Is.EqualTo("5"));
            Assert.That(_livesBar.StatusText, Is.EqualTo("Full"));
        }

        [Test]
        public void Play_ShouldOpenTheLivesPopupInsteadOfTheLevel_WhenNoLivesAreLeft()
        {
            _lives.LoseAll();
            using HomeFlow flow = CreateFlow();
            flow.Start();

            _levelButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.Empty);
            Assert.That(_popups.Current, Is.SameAs(_livesPopup));
            Assert.That(_livesPopup.CountText, Is.EqualTo("0"));
            Assert.That(_livesPopup.TitleText, Is.EqualTo("Out of Lives"));
            Assert.That(_livesPopup.TimerText, Is.EqualTo("Next life in 30:00"));
        }

        [Test]
        public void Refill_ShouldFillTheLivesForCoinsAndClose_WhenThePlayerCanPay()
        {
            _lives.LoseAll();
            _wallet.Add(_lives.Lives.RefillPrice);
            using HomeFlow flow = CreateFlow();
            flow.Start();
            _plusButton.onClick.Invoke();

            _refillButton.onClick.Invoke();

            Assert.That(_lives.Lives.Count, Is.EqualTo(5));
            Assert.That(_wallet.Coins, Is.Zero);
            Assert.That(_popups.Current, Is.Null);
            Assert.That(_livesBar.StatusText, Is.EqualTo("Full"));
        }

        [Test]
        public void LivesPopup_ShouldNotLetThePlayerRefill_WhenTheCoinsAreShort()
        {
            _lives.LoseAll();
            using HomeFlow flow = CreateFlow();
            flow.Start();

            _levelButton.onClick.Invoke();

            Assert.That(_livesPopup.CanRefill, Is.False);
        }

        [Test]
        public void Start_ShouldShowTheLevelPathAndTheBalance_WhenSceneStarts()
        {
            _progress.SetCurrentLevel(23);
            _wallet.Add(1280);
            using HomeFlow flow = CreateFlow();

            flow.Start();

            Assert.That(_view.LevelLabelText, Is.EqualTo("Level 23"));
            Assert.That(_view.PathLabelText(0), Is.EqualTo("23"));
            Assert.That(_view.PathLabelText(1), Is.EqualTo("24"));
            Assert.That(_view.PathLabelText(2), Is.EqualTo("25"));
            Assert.That(_view.CoinText, Is.EqualTo("1.28k"));
        }

        [Test]
        public void LevelButton_ShouldNavigateToGameplay_WhenTapped()
        {
            using HomeFlow flow = CreateFlow();

            _levelButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Gameplay }));
        }

        [Test]
        public void SettingsButton_ShouldOpenTheSettingsShowingTheSavedSwitches_WhenTapped()
        {
            _settings.Set(GameSetting.Music, false);
            using HomeFlow flow = CreateFlow();

            _settingsButton.onClick.Invoke();

            Assert.That(_popups.Current, Is.SameAs(_settingsPopup));
            SettingToggleView music = _toggles[2];
            Assert.That(music.transform.Find("Off").gameObject.activeSelf, Is.True);
        }

        [Test]
        public void SettingsSwitch_ShouldFlipTheSetting_WhenTapped()
        {
            using HomeFlow flow = CreateFlow();
            _settingsButton.onClick.Invoke();

            _toggles[0].Button.onClick.Invoke();

            Assert.That(_settings.IsOn(GameSetting.Vibration), Is.False);
        }

        [Test]
        public void SettingsClose_ShouldCloseThePopup_WhenTapped()
        {
            using HomeFlow flow = CreateFlow();
            _settingsButton.onClick.Invoke();

            _closeButton.onClick.Invoke();

            Assert.That(_popups.IsOpen, Is.False);
        }

        [Test]
        public void Buttons_ShouldDoNothing_WhenFlowIsDisposed()
        {
            CreateFlow().Dispose();

            _levelButton.onClick.Invoke();
            _settingsButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.Empty);
            Assert.That(_popups.Opened, Is.Empty);
        }
    }
}
