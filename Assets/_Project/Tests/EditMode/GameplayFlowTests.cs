using System.Collections.Generic;
using NUnit.Framework;
using RollicCase.Core.Persistence;
using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Tests.EditMode.Helpers;

namespace RollicCase.Tests.EditMode
{
    public sealed class GameplayFlowTests
    {
        [Test]
        public void Start_ShouldFireLevelStartedWithCurrentLevel_WhenSceneStarts()
        {
            var storage = new InMemoryKeyValueStorage();
            var progress = new PlayerProgressService(storage, new RecordingLogger());
            progress.Load();
            progress.AdvanceLevel();
            progress.AdvanceLevel();

            var bus = new SignalBus();
            var received = new List<int>();
            bus.Subscribe<LevelStartedSignal>(signal => received.Add(signal.LevelNumber));

            new GameplayFlow(progress, bus).Start();

            Assert.That(received, Is.EqualTo(new[] { 3 }));
        }
    }
}
