using System.Collections.Generic;
using NUnit.Framework;
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
    public sealed class LevelSessionTests
    {
        TestCatalog _catalogs;
        SignalBus _bus;
        ActiveBoard _board;
        LevelSession _session;
        readonly List<string> _events = new();

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            _board = new ActiveBoard(new BoardSpace(1f), _bus);
            var levels = new LevelProvider(_catalogs.Create(
                new LevelDataBuilder(4, 4).Block(Red, 1, 1).Block(Blue, 3, 3).Door(Left, 1, 1, Red).Build()));
            _session = new LevelSession(_board, levels, _bus);
            _events.Clear();
            _bus.Subscribe<BoardLoadedSignal>(_ => _events.Add("loaded"));
            _bus.Subscribe<LevelStartedSignal>(signal => _events.Add($"started {signal.LevelNumber}"));
            _bus.Subscribe<LevelPausedSignal>(_ => _events.Add("paused"));
            _bus.Subscribe<LevelResumedSignal>(_ => _events.Add("resumed"));
            _bus.Subscribe<LevelCompletedSignal>(signal => _events.Add($"completed {signal.LevelNumber}"));
            _bus.Subscribe<LevelFailedSignal>(signal => _events.Add($"failed {signal.LevelNumber}"));
        }

        [TearDown]
        public void TearDown()
        {
            _session.Dispose();
            _catalogs.Dispose();
        }

        void SlideOutFirstBlock()
        {
            while (_board.TryStep(0, Vector2Int.left) == StepResult.Moved) { }
        }

        [Test]
        public void Session_ShouldBeLoading_WhenCreated()
        {
            Assert.That(_session.State, Is.EqualTo(LevelState.Loading));
            Assert.That(_session.LevelNumber, Is.Zero);
        }

        [Test]
        public void Start_ShouldLoadTheBoardThenPlay_WhenLoading()
        {
            Assert.That(_session.Start(3), Is.True);

            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_session.LevelNumber, Is.EqualTo(3));
            Assert.That(_events, Is.EqualTo(new[] { "loaded", "started 3" }));
        }

        [Test]
        public void Start_ShouldBeRefused_WhenAnAttemptIsUnderway()
        {
            _session.Start(1);
            _events.Clear();

            Assert.That(_session.Start(2), Is.False);
            Assert.That(_session.LevelNumber, Is.EqualTo(1));
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void PauseAndResume_ShouldToggleBetweenPlayingAndPaused_WhenLegal()
        {
            _session.Start(1);
            _events.Clear();

            Assert.That(_session.Pause(), Is.True);
            Assert.That(_session.State, Is.EqualTo(LevelState.Paused));
            Assert.That(_session.Pause(), Is.False, "Already paused.");
            Assert.That(_session.Resume(), Is.True);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_session.Resume(), Is.False, "Not paused.");
            Assert.That(_events, Is.EqualTo(new[] { "paused", "resumed" }));
        }

        [Test]
        public void Session_ShouldBeWon_WhenTheBoardIsCleared()
        {
            var levels = new LevelProvider(_catalogs.Create(
                new LevelDataBuilder(4, 4).Block(Red, 1, 1).Door(Left, 1, 1, Red).Build()));
            using var session = new LevelSession(_board, levels, _bus);
            session.Start(1);
            _events.Clear();

            SlideOutFirstBlock();

            Assert.That(session.State, Is.EqualTo(LevelState.Won));
            Assert.That(_events, Is.EqualTo(new[] { "completed 1" }));
            Assert.That(session.Pause(), Is.False, "Nothing pauses a won level.");
            Assert.That(session.Fail(), Is.False, "A late timer cannot fail a won level.");
            Assert.That(session.Restart(), Is.False, "A won level moves on; it does not restart.");
        }

        [Test]
        public void Fail_ShouldEndTheAttemptOnce_WhenPlaying()
        {
            _session.Start(2);
            _events.Clear();

            Assert.That(_session.Fail(), Is.True);
            Assert.That(_session.Fail(), Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Failed));
            Assert.That(_session.Pause(), Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "failed 2" }));
        }

        [Test]
        public void Fail_ShouldBeRefused_WhenPausedOrNotStarted()
        {
            Assert.That(_session.Fail(), Is.False, "Not started.");
            _session.Start(1);
            _session.Pause();

            Assert.That(_session.Fail(), Is.False, "The timer stands still while paused.");
            Assert.That(_session.State, Is.EqualTo(LevelState.Paused));
        }

        [Test]
        public void Restart_ShouldGiveAFreshBoardEqualToAFirstStart_WhenPlayingPausedOrFailed()
        {
            _session.Start(1);
            BoardBlock[] fresh = { _board.Current.Blocks[0], _board.Current.Blocks[1] };
            Vector2Int[] freshCells = { fresh[0].GetCell(0), fresh[1].GetCell(0) };
            _board.TryStep(0, Vector2Int.left);
            _board.TryStep(0, Vector2Int.left);
            Assert.That(_board.Current.Blocks[0].HasExited, Is.True, "Precondition: the attempt changed the board.");

            foreach (System.Func<bool> before in new System.Func<bool>[] { () => true, _session.Pause, _session.Fail })
            {
                before();
                _events.Clear();

                Assert.That(_session.Restart(), Is.True);

                Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
                Assert.That(_events, Is.EqualTo(new[] { "loaded", "started 1" }));
                Assert.That(_board.Current.RemainingBlocks, Is.EqualTo(2));
                Assert.That(_board.Current.Blocks[0].HasExited, Is.False);
                Assert.That(_board.Current.Blocks[0].GetCell(0), Is.EqualTo(freshCells[0]));
                Assert.That(_board.Current.Blocks[1].GetCell(0), Is.EqualTo(freshCells[1]));
                Assert.That(_board.Current.Blocks[0], Is.Not.SameAs(fresh[0]), "A new board, not the old one reset.");
                _board.TryStep(0, Vector2Int.right);
            }
        }

        [Test]
        public void Restart_ShouldBeRefused_WhenNotStarted()
        {
            Assert.That(_session.Restart(), Is.False);
            Assert.That(_events, Is.Empty);
        }
    }
}
