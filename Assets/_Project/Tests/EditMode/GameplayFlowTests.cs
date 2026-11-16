using System.Collections.Generic;
using NUnit.Framework;
using RollicCase.Core.Persistence;
using RollicCase.Core.Scenes;
using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Tests.EditMode.Helpers;

namespace RollicCase.Tests.EditMode
{
    public sealed class GameplayFlowTests
    {
        TestViews _views;
        RecordingNavigation _navigation;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _navigation = new RecordingNavigation();
        }

        [TearDown]
        public void TearDown() => _views.Dispose();

        static PlayerProgressService CreateProgressAtLevel(int level)
        {
            var progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            progress.Load();
            for (int i = 1; i < level; i++)
                progress.AdvanceLevel();
            return progress;
        }

        [Test]
        public void Start_ShouldFireLevelStartedWithCurrentLevel_WhenSceneStarts()
        {
            var bus = new SignalBus();
            var received = new List<int>();
            bus.Subscribe<LevelStartedSignal>(signal => received.Add(signal.LevelNumber));
            // The HUD listens on its own bus so the test observes the flow, not the HUD's reveal.
            var hud = _views.CreateHud(new SignalBus(), out _);

            new GameplayFlow(CreateProgressAtLevel(3), bus, hud, _navigation).Start();

            Assert.That(received, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void HomeButton_ShouldNavigateHome_WhenTapped()
        {
            var hud = _views.CreateHud(new SignalBus(), out var homeButton);
            using var flow = new GameplayFlow(CreateProgressAtLevel(1), new SignalBus(), hud, _navigation);

            homeButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void HomeButton_ShouldNotNavigate_WhenFlowIsDisposed()
        {
            var hud = _views.CreateHud(new SignalBus(), out var homeButton);
            new GameplayFlow(CreateProgressAtLevel(1), new SignalBus(), hud, _navigation).Dispose();

            homeButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.Empty);
        }
    }
}
