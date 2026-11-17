using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelProgressionTests
    {
        [Test]
        public void Progression_ShouldAdvanceTheLevelOnce_WhenALevelIsWon()
        {
            var bus = new SignalBus();
            var progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            progress.Load();
            using var progression = new LevelProgression(bus, progress);

            bus.Fire(new LevelCompletedSignal(1));

            Assert.That(progress.CurrentLevel, Is.EqualTo(2));
        }

        [Test]
        public void Progression_ShouldNotAdvance_WhenALevelIsLostOrDisposed()
        {
            var bus = new SignalBus();
            var progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            progress.Load();
            var progression = new LevelProgression(bus, progress);

            bus.Fire(new LevelFailedSignal(1));
            progression.Dispose();
            bus.Fire(new LevelCompletedSignal(1));

            Assert.That(progress.CurrentLevel, Is.EqualTo(1));
        }
    }
}
