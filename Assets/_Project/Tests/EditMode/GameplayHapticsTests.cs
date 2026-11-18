using NUnit.Framework;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Feedback;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using SettingsService = ColorBlockJam.Core.Persistence.SettingsService;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class GameplayHapticsTests
    {
        static readonly LevelData Level = new LevelDataBuilder(5, 4)
            .Block(Red, 0, 0)
            .Block(Blue, 0, 2, 1, 2)
            .Door(Left, 0, 1, Red)
            .Door(Left, 2, 1, Blue)
            .Build();

        SignalBus _bus;
        RecordingHaptics _haptics;
        GameplayConfig _config;
        BoardViewConfig _boardView;
        SettingsService _settings;
        FakeGameClock _clock;
        ActiveBoard _board;
        BoardSpace _space;
        GameplayHaptics _gameplayHaptics;

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _haptics = new RecordingHaptics();
            _config = ScriptableObject.CreateInstance<GameplayConfig>();
            var serialized = new SerializedObject(_config);
            serialized.FindProperty("_exitTickInterval").floatValue = 0.05f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _boardView = ScriptableObject.CreateInstance<BoardViewConfig>();
            _settings = new SettingsService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _settings.Load();
            _clock = new FakeGameClock();
            _space = new BoardSpace(1f);
            _board = new ActiveBoard(_space, _bus);
            _board.Load(Level);
            _gameplayHaptics = Create();
        }

        GameplayHaptics Create() =>
            new(_bus, _haptics, _config, _boardView, _settings, _board, _space, _clock);

        [TearDown]
        public void TearDown()
        {
            _gameplayHaptics.Dispose();
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_boardView);
        }

        float TrillSeconds(int length) => _boardView.ExitSnapDuration + _boardView.ExitSlideSeconds(length);

        int TicksOver(float seconds) => Mathf.CeilToInt(seconds / _config.ExitTickInterval);

        void Exit(int blockId) => _bus.Fire(new BlockExitedSignal(blockId, Vector2Int.left, Vector2Int.zero));

        [Test]
        public void Haptics_ShouldTapTheGrabImpact_WhenABlockIsPicked()
        {
            _bus.Fire(new BlockGrabbedSignal(0));

            Assert.That(_haptics.Played, Is.EqualTo(new[] { _config.GrabHaptic }));
        }

        [Test]
        public void Haptics_ShouldTapTheWinAndFailImpacts_WhenTheLevelEnds()
        {
            _bus.Fire(new LevelCompletedSignal(1));
            _bus.Fire(new LevelFailedSignal(1));

            Assert.That(_haptics.Played, Is.EqualTo(new[] { _config.WinHaptic, _config.FailHaptic }));
        }

        [Test]
        public void Haptics_ShouldEndTheTrill_WhenTheLevelIsWonWhileABlockSlidesOut()
        {
            Exit(0);
            _bus.Fire(new LevelCompletedSignal(1));

            _clock.Advance(TrillSeconds(1) + 1f);

            Assert.That(_haptics.Played, Is.EqualTo(new[] { _config.ExitHaptic, _config.WinHaptic }));
        }

        [Test]
        public void Haptics_ShouldTickUntilTheBlockHasSlidOut_WhenABlockLeaves()
        {
            Exit(0);
            Assert.That(_haptics.Played, Is.EqualTo(new[] { _config.ExitHaptic }), "The first tick comes at once.");

            _clock.Advance(TrillSeconds(1) + 1f);

            Assert.That(_haptics.Played, Has.Count.EqualTo(TicksOver(TrillSeconds(1))));
            Assert.That(_haptics.Played, Is.All.EqualTo(_config.ExitHaptic));
        }

        [Test]
        public void Haptics_ShouldTickLonger_WhenTheLeavingBlockIsLonger()
        {
            Exit(1);
            _clock.Advance(TrillSeconds(2) + 1f);

            Assert.That(_haptics.Played, Has.Count.EqualTo(TicksOver(TrillSeconds(2))));
            Assert.That(TicksOver(TrillSeconds(2)), Is.GreaterThan(TicksOver(TrillSeconds(1))));
        }

        [Test]
        public void Haptics_ShouldPlayOneTrill_WhenTwoBlocksLeaveTogether()
        {
            Exit(0);
            Exit(1);
            _clock.Advance(TrillSeconds(2) + 1f);

            Assert.That(_haptics.Played, Has.Count.EqualTo(TicksOver(TrillSeconds(2))));
        }

        [Test]
        public void Haptics_ShouldStartANewTrill_WhenABlockLeavesAfterTheLastOneEnded()
        {
            Exit(0);
            _clock.Advance(TrillSeconds(1) + 1f);
            int first = _haptics.Played.Count;

            Exit(0);

            Assert.That(_haptics.Played, Has.Count.EqualTo(first + 1));
        }

        [Test]
        public void Haptics_ShouldStopTheTrill_WhenDisposed()
        {
            Exit(0);
            _gameplayHaptics.Dispose();

            _clock.Advance(TrillSeconds(1) + 1f);

            Assert.That(_haptics.Played, Has.Count.EqualTo(1));
            _gameplayHaptics = Create();
        }

        [Test]
        public void Haptics_ShouldStopListening_WhenDisposed()
        {
            _gameplayHaptics.Dispose();

            _bus.Fire(new BlockGrabbedSignal(0));
            Exit(0);
            _bus.Fire(new LevelCompletedSignal(1));

            Assert.That(_haptics.Played, Is.Empty);
            _gameplayHaptics = Create();
        }

        [Test]
        public void Haptics_ShouldStayStill_WhenVibrationIsOff()
        {
            _settings.Set(GameSetting.Vibration, false);

            _bus.Fire(new BlockGrabbedSignal(0));
            Exit(0);
            _clock.Advance(TrillSeconds(1) + 1f);
            _bus.Fire(new LevelFailedSignal(1));

            Assert.That(_haptics.Played, Is.Empty);
        }

        [Test]
        public void ShippedConfig_ShouldTickLightlyAndTapHarderOnGrabAndResults_WhenTheGameShips()
        {
            var shipped = AssetDatabase.LoadAssetAtPath<GameplayConfig>("Assets/_Project/Configs/GameplayConfig.asset");

            Assert.That(shipped.ExitHaptic, Is.EqualTo(HapticImpact.Light));
            Assert.That(shipped.GrabHaptic, Is.EqualTo(HapticImpact.Medium));
            Assert.That(shipped.WinHaptic, Is.EqualTo(HapticImpact.Heavy));
            Assert.That(shipped.FailHaptic, Is.EqualTo(HapticImpact.Heavy));
            Assert.That(shipped.ExitTickInterval, Is.InRange(0.04f, 0.1f));
        }
    }
}
