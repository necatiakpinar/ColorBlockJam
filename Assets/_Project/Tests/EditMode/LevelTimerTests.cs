using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelTimerTests
    {
        const int TimerSeconds = 12;

        TestCatalog _catalogs;
        SignalBus _bus;
        ActiveBoard _board;
        LevelSession _session;
        FakeGameClock _clock;
        LevelTimer _timer;
        readonly List<int> _shown = new();
        int _fails;

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            _board = new ActiveBoard(new BoardSpace(1f), _bus);
            var levels = new LevelProvider(_catalogs.Create(new LevelDataBuilder(4, 4).Timer(TimerSeconds)
                .Block(Red, 1, 1).Block(Blue, 3, 3).Door(Left, 1, 1, Red).Door(Right, 3, 1, Blue).Build()));
            _session = new LevelSession(_board, levels, _bus);
            _clock = new FakeGameClock();
            _timer = new LevelTimer(_session, _clock, _bus);
            _shown.Clear();
            _fails = 0;
            _bus.Subscribe<TimerChangedSignal>(signal => _shown.Add(signal.SecondsLeft));
            _bus.Subscribe<LevelFailedSignal>(_ => _fails++);
        }

        [TearDown]
        public void TearDown()
        {
            _timer.Dispose();
            _session.Dispose();
            _catalogs.Dispose();
        }

        void StartAndTouch()
        {
            _session.Start(1);
            Grab();
        }

        void Grab() => _bus.Fire(new BlockGrabbedSignal(0));

        [Test]
        public void Timer_ShouldShowTheFullTimeAndWait_UntilTheFirstGrab()
        {
            _session.Start(1);
            _clock.Advance(30f);
            float beforeTouch = _timer.SecondsLeft;

            Grab();
            _clock.Advance(1f);

            Assert.That(beforeTouch, Is.EqualTo(TimerSeconds));
            Assert.That(_fails, Is.Zero);
            Assert.That(_shown, Is.EqualTo(new[] { TimerSeconds, TimerSeconds - 1 }));
        }

        [Test]
        public void Timer_ShouldKeepCounting_WhenLaterGrabsHappen()
        {
            StartAndTouch();
            _clock.Advance(2f);

            _bus.Fire(new BlockGrabbedSignal(1));
            _clock.Advance(1f);

            Assert.That(_timer.SecondsLeft, Is.EqualTo(TimerSeconds - 3f).Within(1e-4f));
        }

        [Test]
        public void Timer_ShouldAnnounceEachSecondOnce_WhenTimePasses()
        {
            StartAndTouch();

            _clock.Advance(0.5f);
            _clock.Advance(0.5f);
            _clock.Advance(2f);

            Assert.That(_shown, Is.EqualTo(new[] { 12, 11, 10, 9 }));
            Assert.That(_timer.SecondsLeft, Is.EqualTo(9f).Within(1e-4f));
        }

        [Test]
        public void Timer_ShouldStandStill_WhileTheLevelIsPaused()
        {
            StartAndTouch();
            _clock.Advance(2.5f);

            _session.Pause();
            _clock.Advance(30f);
            int shownWhilePaused = _shown.Count;
            _session.Resume();
            _clock.Advance(0.5f);

            Assert.That(shownWhilePaused, Is.EqualTo(3), "12, 11, 10 and nothing while paused.");
            Assert.That(_fails, Is.Zero);
            Assert.That(_shown[_shown.Count - 1], Is.EqualTo(9), "Picks up where it stopped.");
            Assert.That(_timer.SecondsLeft, Is.EqualTo(9f).Within(1e-4f));
        }

        [Test]
        public void Timer_ShouldStandStill_WhileTheAppIsInTheBackground()
        {
            StartAndTouch();
            _clock.Advance(1f);

            _bus.Fire(new ApplicationPausedSignal(true));
            _clock.Advance(100f);
            float whileAway = _timer.SecondsLeft;
            _bus.Fire(new ApplicationPausedSignal(false));
            _clock.Advance(1f);

            Assert.That(whileAway, Is.EqualTo(11f).Within(1e-4f));
            Assert.That(_timer.SecondsLeft, Is.EqualTo(10f).Within(1e-4f));
            Assert.That(_fails, Is.Zero);
        }

        [Test]
        public void Timer_ShouldStayFrozen_WhenTheAppComesBackWhileTheLevelIsPaused()
        {
            StartAndTouch();
            _session.Pause();
            _bus.Fire(new ApplicationPausedSignal(true));
            _bus.Fire(new ApplicationPausedSignal(false));

            _clock.Advance(5f);

            Assert.That(_timer.SecondsLeft, Is.EqualTo(TimerSeconds));
        }

        [Test]
        public void Timer_ShouldFailTheLevelOnce_WhenItReachesZero()
        {
            StartAndTouch();

            _clock.Advance(TimerSeconds);
            _clock.Advance(10f);

            Assert.That(_fails, Is.EqualTo(1));
            Assert.That(_session.State, Is.EqualTo(LevelState.Failed));
            Assert.That(_shown[_shown.Count - 1], Is.Zero);
            Assert.That(_timer.SecondsLeft, Is.Zero);
        }

        [Test]
        public void Timer_ShouldStop_WhenTheLevelIsWon()
        {
            StartAndTouch();
            _clock.Advance(3f);

            while (_board.TryStep(0, Vector2Int.left) == StepResult.Moved) { }
            while (_board.TryStep(1, Vector2Int.right) == StepResult.Moved) { }
            _clock.Advance(TimerSeconds);

            Assert.That(_session.State, Is.EqualTo(LevelState.Won));
            Assert.That(_fails, Is.Zero);
            Assert.That(_timer.SecondsLeft, Is.EqualTo(9f).Within(1e-4f));
        }

        [Test]
        public void Timer_ShouldStartFullAndWaitForAGrabAgain_WhenTheLevelRestarts()
        {
            StartAndTouch();
            _clock.Advance(TimerSeconds);

            _session.Restart();
            _clock.Advance(5f);
            float beforeTouch = _timer.SecondsLeft;
            Grab();
            _clock.Advance(1f);

            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(beforeTouch, Is.EqualTo(TimerSeconds), "Full again, waiting for a grab.");
            Assert.That(_timer.SecondsLeft, Is.EqualTo(TimerSeconds - 1f).Within(1e-4f));
            Assert.That(_shown.GetRange(_shown.Count - 2, 2), Is.EqualTo(new[] { TimerSeconds, TimerSeconds - 1 }));
        }

        [Test]
        public void Timer_ShouldStopCounting_WhenDisposed()
        {
            StartAndTouch();
            _timer.Dispose();

            _clock.Advance(TimerSeconds * 2f);

            Assert.That(_fails, Is.Zero);
            Assert.That(_shown, Is.EqualTo(new[] { TimerSeconds }));
        }
    }
}
