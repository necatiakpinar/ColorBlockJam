using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.UI;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine.UI;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>Starting the level and routing the HUD and the pause popup (gameplay.md §7).</summary>
    public sealed class GameplayFlowTests
    {
        TestViews _views;
        TestCatalog _catalogs;
        RecordingNavigation _navigation;
        RecordingPopupService _popups;
        SignalBus _bus;
        ActiveBoard _activeBoard;
        LevelSession _session;
        BoosterRig _boosters;
        SettingsService _settings;
        Button _restartButton;
        Button _pauseButton;
        Button _closeButton;
        Button _homeButton;
        SettingToggleView[] _toggles;
        PausePopupView _pausePopup;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _catalogs = new TestCatalog();
            _navigation = new RecordingNavigation();
            _popups = new RecordingPopupService();
            _bus = new SignalBus();
            var levels = new LevelProvider(_catalogs.Create(
                new LevelDataBuilder(4, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build(),
                new LevelDataBuilder(5, 6).Block(Blue, 0, 0).Door(Left, 0, 1, Blue).Build()));
            _activeBoard = new ActiveBoard(new BoardSpace(1f), _bus);
            _session = new LevelSession(_activeBoard, levels, _bus);
            _boosters = new BoosterRig(_activeBoard, _session, _bus);
            _settings = new SettingsService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _settings.Load();
        }

        [TearDown]
        public void TearDown()
        {
            _boosters.Dispose();
            _session.Dispose();
            _views.Dispose();
            _catalogs.Dispose();
        }

        static PlayerProgressService CreateProgressAtLevel(int level)
        {
            var progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            progress.Load();
            for (int i = 1; i < level; i++)
                progress.AdvanceLevel();
            return progress;
        }

        GameplayFlow CreateFlow(int level)
        {
            var wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            wallet.Load();
            // The HUD listens on its own bus so these tests observe the flow, not the HUD's reveal.
            var hud = _views.CreateHud(new SignalBus(), wallet, out _restartButton, out _pauseButton);
            _pausePopup = _views.CreatePausePopup(_bus, _settings, out _closeButton, out _homeButton, out _toggles);
            return new GameplayFlow(CreateProgressAtLevel(level), _session, hud, _pausePopup, _popups, _settings, _navigation,
                _boosters.Service);
        }

        [Test]
        public void Start_ShouldLoadTheBoardThenAnnounceTheLevel_WhenSceneStarts()
        {
            var events = new List<string>();
            _bus.Subscribe<BoardLoadedSignal>(signal => events.Add($"board {signal.Level.Width}x{signal.Level.Height}"));
            _bus.Subscribe<LevelStartedSignal>(signal => events.Add($"level {signal.LevelNumber}"));

            CreateFlow(2).Start();

            Assert.That(events, Is.EqualTo(new[] { "board 5x6", "level 2" }));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
        }

        [Test]
        public void RestartButton_ShouldRestartTheLevel_WhenTapped()
        {
            using GameplayFlow flow = CreateFlow(1);
            flow.Start();
            int starts = 0;
            _bus.Subscribe<LevelStartedSignal>(_ => starts++);

            _restartButton.onClick.Invoke();

            Assert.That(starts, Is.EqualTo(1));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
        }

        [Test]
        public void PauseButton_ShouldPauseThenOpenThePausePopup_WhenTapped()
        {
            using GameplayFlow flow = CreateFlow(1);
            flow.Start();

            _pauseButton.onClick.Invoke();

            Assert.That(_session.State, Is.EqualTo(LevelState.Paused));
            Assert.That(_popups.Current, Is.SameAs(_pausePopup));
        }

        [Test]
        public void PausePopupClose_ShouldCloseAndResume_WhenTapped()
        {
            using GameplayFlow flow = CreateFlow(1);
            flow.Start();
            _pauseButton.onClick.Invoke();

            _closeButton.onClick.Invoke();

            Assert.That(_popups.IsOpen, Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
        }

        [Test]
        public void PauseButton_ShouldDoNothing_WhenTheLevelIsNotPlaying()
        {
            using GameplayFlow flow = CreateFlow(1);
            flow.Start();
            _session.Fail();

            _pauseButton.onClick.Invoke();

            Assert.That(_popups.Opened, Is.Empty);
        }

        [Test]
        public void Toggle_ShouldFlipAndShowTheSetting_WhenTapped()
        {
            using GameplayFlow flow = CreateFlow(1);
            flow.Start();
            _pauseButton.onClick.Invoke();
            SettingToggleView sound = _toggles[1];

            sound.Button.onClick.Invoke();

            Assert.That(_settings.IsOn(GameSetting.Sound), Is.False);
            Assert.That(sound.transform.Find("Off").gameObject.activeSelf, Is.True);
            Assert.That(sound.transform.Find("On").gameObject.activeSelf, Is.False);
            Assert.That(_settings.IsOn(GameSetting.Music), Is.True, "Only the tapped switch changes.");
        }

        [Test]
        public void PausePopupHome_ShouldNavigateHome_WhenTapped()
        {
            using GameplayFlow flow = CreateFlow(1);
            flow.Start();
            _pauseButton.onClick.Invoke();

            _homeButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void Buttons_ShouldDoNothing_WhenTheFlowIsDisposed()
        {
            GameplayFlow flow = CreateFlow(1);
            flow.Start();
            flow.Dispose();

            _pauseButton.onClick.Invoke();
            _homeButton.onClick.Invoke();

            Assert.That(_popups.Opened, Is.Empty);
            Assert.That(_navigation.Requests, Is.Empty);
        }
    }
}
