using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class GameplayFlowTests
    {
        TestViews _views;
        TestCatalog _catalogs;
        RecordingNavigation _navigation;
        SignalBus _bus;
        LevelProvider _levels;
        ActiveBoard _activeBoard;
        LevelSession _session;
        GameplayConfig _config;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _catalogs = new TestCatalog();
            _navigation = new RecordingNavigation();
            _bus = new SignalBus();
            _levels = new LevelProvider(_catalogs.Create(
                new LevelDataBuilder(4, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build(),
                new LevelDataBuilder(5, 6).Block(Blue, 0, 0).Door(Left, 0, 1, Blue).Build()));
            _activeBoard = new ActiveBoard(new BoardSpace(1f), _bus);
            _session = new LevelSession(_activeBoard, _levels, _bus);
            _config = ScriptableObject.CreateInstance<GameplayConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            _session.Dispose();
            Object.DestroyImmediate(_config);
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

        // The HUD listens on its own bus so these tests observe the flow, not the HUD's reveal.
        GameplayHudView CreateHud(out UnityEngine.UI.Button homeButton) => _views.CreateHud(new SignalBus(), out homeButton);

        GameplayFlow CreateFlow(int level) =>
            new(CreateProgressAtLevel(level), _session, CreateHud(out _), _navigation, _bus, _config);

        [Test]
        public void Start_ShouldLoadTheBoardThenAnnounceTheLevel_WhenSceneStarts()
        {
            var events = new List<string>();
            _bus.Subscribe<BoardLoadedSignal>(signal => events.Add($"board {signal.Level.Width}x{signal.Level.Height}"));
            _bus.Subscribe<LevelStartedSignal>(signal => events.Add($"level {signal.LevelNumber}"));

            CreateFlow(2).Start();

            Assert.That(events, Is.EqualTo(new[] { "board 5x6", "level 2" }));
            Assert.That(_activeBoard.Current.Width, Is.EqualTo(5));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
        }

        [Test]
        public void HomeButton_ShouldNavigateHome_WhenTapped()
        {
            var hud = CreateHud(out var homeButton);
            using var flow = new GameplayFlow(CreateProgressAtLevel(1), _session, hud, _navigation, _bus, _config);

            homeButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void HomeButton_ShouldNotNavigate_WhenFlowIsDisposed()
        {
            var hud = CreateHud(out var homeButton);
            new GameplayFlow(CreateProgressAtLevel(1), _session, hud, _navigation, _bus, _config).Dispose();

            homeButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.Empty);
        }
    }
}
