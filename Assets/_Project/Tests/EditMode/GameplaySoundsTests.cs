using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Audio;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Feedback;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class GameplaySoundsTests
    {
        SignalBus _bus;
        RecordingAudio _audio;
        GameplayConfig _config;
        GameplaySounds _sounds;

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _audio = new RecordingAudio();
            _config = ScriptableObject.CreateInstance<GameplayConfig>();
            _sounds = new GameplaySounds(_bus, _audio, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _sounds.Dispose();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Sounds_ShouldPlayTheirCue_WhenTheGameplayEventHappens()
        {
            _bus.Fire(new BlockGrabbedSignal(0));
            _bus.Fire(new BlockExitedSignal(0, Vector2Int.left, Vector2Int.zero));
            _bus.Fire(new BoosterUsedSignal(BoosterType.Hammer));
            _bus.Fire(new LevelCompletedSignal(1));
            _bus.Fire(new LevelFailedSignal(1));

            Assert.That(_audio.Played, Is.EqualTo(new[]
            {
                AudioCue.Grab, AudioCue.Exit, AudioCue.Booster, AudioCue.Win, AudioCue.Fail,
            }));
        }

        [TestCase(11, false)]
        [TestCase(10, true)]
        [TestCase(1, true)]
        [TestCase(0, false)]
        public void Tick_ShouldPlayOnlyInTheLastTenSeconds_WhenTheTimerChanges(int secondsLeft, bool ticks)
        {
            _bus.Fire(new TimerChangedSignal(secondsLeft));

            Assert.That(_audio.Played.Contains(AudioCue.Tick), Is.EqualTo(ticks));
        }

        [Test]
        public void Sounds_ShouldStopListening_WhenDisposed()
        {
            _sounds.Dispose();

            _bus.Fire(new BlockGrabbedSignal(0));

            Assert.That(_audio.Played, Is.Empty);
            _sounds = new GameplaySounds(_bus, _audio, _config);
        }
    }
}
