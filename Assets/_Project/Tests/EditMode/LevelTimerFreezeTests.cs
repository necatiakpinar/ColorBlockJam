using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The freeze booster holding the level timer still (gameplay.md §6, DA-36), on a fake clock.</summary>
    public sealed class LevelTimerFreezeTests
    {
        const int TimerSeconds = 30;
        const float FreezeSeconds = 10f;

        TestCatalog _catalogs;
        SignalBus _bus;
        LevelSession _session;
        FakeGameClock _clock;
        LevelTimer _timer;
        readonly List<string> _freezes = new();

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            var board = new ActiveBoard(new BoardSpace(1f), _bus);
            var levels = new LevelProvider(_catalogs.Create(new LevelDataBuilder(4, 4).Timer(TimerSeconds)
                .Block(Red, 1, 1).Block(Blue, 3, 3).Door(Left, 1, 1, Red).Door(Right, 3, 1, Blue).Build()));
            _session = new LevelSession(board, levels, _bus);
            _clock = new FakeGameClock();
            _timer = new LevelTimer(_session, _clock, _bus);
            _freezes.Clear();
            _bus.Subscribe<TimerFreezeChangedSignal>(signal =>
                _freezes.Add($"{signal.SecondsLeft:0.#}/{signal.TotalSeconds:0.#} {(signal.Counting ? "counting" : "still")}"));
            _session.Start(1);
            _bus.Fire(new BlockGrabbedSignal(0));
        }

        [TearDown]
        public void TearDown()
        {
            _timer.Dispose();
            _session.Dispose();
            _catalogs.Dispose();
        }

        [Test]
        public void Freeze_ShouldHoldTheTimerForItsSecondsThenLetItRun_WhenUsed()
        {
            _clock.Advance(3f);

            _timer.FreezeFor(FreezeSeconds);
            _clock.Advance(FreezeSeconds);
            bool frozenAtTheEnd = _timer.IsFrozen;
            _clock.Advance(2f);

            Assert.That(frozenAtTheEnd, Is.False);
            Assert.That(_timer.SecondsLeft, Is.EqualTo(TimerSeconds - 5f).Within(1e-4f));
            Assert.That(_freezes, Is.EqualTo(new[] { "10/10 still", "10/10 counting", "0/10 still" }));
        }

        [Test]
        public void Freeze_ShouldStandStillWithTheLevel_WhenPausedMidway()
        {
            _timer.FreezeFor(FreezeSeconds);
            _clock.Advance(4f);

            _session.Pause();
            _clock.Advance(20f);
            float leftWhilePaused = _timer.FreezeSecondsLeft;
            _session.Resume();
            _clock.Advance(6f);

            Assert.That(leftWhilePaused, Is.EqualTo(6f).Within(1e-4f));
            Assert.That(_timer.IsFrozen, Is.False);
            Assert.That(_timer.SecondsLeft, Is.EqualTo(TimerSeconds).Within(1e-4f));
        }

        [Test]
        public void Freeze_ShouldStandStill_WhileTheAppIsInTheBackground()
        {
            _timer.FreezeFor(FreezeSeconds);
            _clock.Advance(1f);

            _bus.Fire(new ApplicationPausedSignal(true));
            _clock.Advance(30f);

            Assert.That(_timer.FreezeSecondsLeft, Is.EqualTo(9f).Within(1e-4f));
        }

        [Test]
        public void Freeze_ShouldNotStack_WhenOneIsRunning()
        {
            _timer.FreezeFor(FreezeSeconds);

            bool second = _timer.FreezeFor(FreezeSeconds);

            Assert.That(second, Is.False);
            Assert.That(_timer.FreezeSecondsLeft, Is.EqualTo(FreezeSeconds).Within(1e-4f));
        }

        [Test]
        public void Restart_ShouldEndTheFreeze_WhenALevelStartsAgain()
        {
            _timer.FreezeFor(FreezeSeconds);
            _clock.Advance(2f);

            _session.Restart();

            Assert.That(_timer.IsFrozen, Is.False);
            Assert.That(_freezes[^1], Is.EqualTo("0/10 still"));
        }
    }
}
