using NUnit.Framework;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Feedback;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class GameplayHapticsTests
    {
        SignalBus _bus;
        RecordingHaptics _haptics;
        GameplayConfig _config;
        GameplayHaptics _gameplayHaptics;

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _haptics = new RecordingHaptics();
            _config = ScriptableObject.CreateInstance<GameplayConfig>();
            _gameplayHaptics = new GameplayHaptics(_bus, _haptics, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _gameplayHaptics.Dispose();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Haptics_ShouldTapFirmly_WhenADoorRejectsAPush()
        {
            _bus.Fire(new BlockRejectedSignal(0, Vector2Int.left));

            Assert.That(_haptics.Played, Is.EqualTo(new[] { _config.RejectHaptic }));
            Assert.That(_config.RejectHaptic, Is.EqualTo(HapticImpact.Medium));
        }

        [Test]
        public void Haptics_ShouldTapLightly_WhenABlockLeaves()
        {
            _bus.Fire(new BlockExitedSignal(0, Vector2Int.left, Vector2Int.zero));

            Assert.That(_haptics.Played, Is.EqualTo(new[] { HapticImpact.Light }));
        }

        [Test]
        public void Haptics_ShouldStopListening_WhenDisposed()
        {
            _gameplayHaptics.Dispose();

            _bus.Fire(new BlockRejectedSignal(0, Vector2Int.left));

            Assert.That(_haptics.Played, Is.Empty);
            _gameplayHaptics = new GameplayHaptics(_bus, _haptics, _config);
        }
    }
}
