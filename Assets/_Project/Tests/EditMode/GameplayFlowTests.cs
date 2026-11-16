using System.Collections.Generic;
using NUnit.Framework;
using RollicCase.Core.Persistence;
using RollicCase.Core.Scenes;
using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay;
using RollicCase.Game.Gameplay.Board;
using RollicCase.Game.Gameplay.Hud;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Game.Levels;
using RollicCase.Tests.EditMode.Helpers;
using UnityEngine;
using static RollicCase.Game.Levels.BlockColor;
using static RollicCase.Game.Levels.WallSide;

namespace RollicCase.Tests.EditMode
{
    public sealed class GameplayFlowTests
    {
        TestViews _views;
        TestCatalog _catalogs;
        RecordingNavigation _navigation;
        SignalBus _bus;
        LevelProvider _levels;
        ActiveBoard _activeBoard;

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
        }

        [TearDown]
        public void TearDown()
        {
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
            new(CreateProgressAtLevel(level), _bus, CreateHud(out _), _navigation, _levels, _activeBoard);

        [Test]
        public void Start_ShouldLoadTheBoardThenAnnounceTheLevel_WhenSceneStarts()
        {
            var events = new List<string>();
            _bus.Subscribe<BoardLoadedSignal>(signal => events.Add($"board {signal.Level.Width}x{signal.Level.Height}"));
            _bus.Subscribe<LevelStartedSignal>(signal => events.Add($"level {signal.LevelNumber}"));

            CreateFlow(2).Start();

            Assert.That(events, Is.EqualTo(new[] { "board 5x6", "level 2" }));
            Assert.That(_activeBoard.Current.Width, Is.EqualTo(5));
        }

        [Test]
        public void HomeButton_ShouldNavigateHome_WhenTapped()
        {
            var hud = CreateHud(out var homeButton);
            using var flow = new GameplayFlow(CreateProgressAtLevel(1), _bus, hud, _navigation, _levels, _activeBoard);

            homeButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void HomeButton_ShouldNotNavigate_WhenFlowIsDisposed()
        {
            var hud = CreateHud(out var homeButton);
            new GameplayFlow(CreateProgressAtLevel(1), _bus, hud, _navigation, _levels, _activeBoard).Dispose();

            homeButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.Empty);
        }
    }
}
