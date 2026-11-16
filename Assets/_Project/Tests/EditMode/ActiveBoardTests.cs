using System.Collections.Generic;
using NUnit.Framework;
using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay.Board;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Game.Levels;
using RollicCase.Tests.EditMode.Helpers;
using UnityEngine;
using static RollicCase.Game.Levels.BlockColor;
using static RollicCase.Game.Levels.WallSide;

namespace RollicCase.Tests.EditMode
{
    public sealed class ActiveBoardTests
    {
        static readonly LevelData Level = new LevelDataBuilder(5, 6).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build();

        SignalBus _bus;
        BoardSpace _space;
        ActiveBoard _board;
        readonly List<string> _events = new();

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _space = new BoardSpace(1f);
            _board = new ActiveBoard(_space, _bus);
            _events.Clear();
            _bus.Subscribe<BoardLoadedSignal>(signal => _events.Add($"loaded {signal.Level.Width}x{signal.Level.Height}"));
            _bus.Subscribe<BlockMovedSignal>(signal => _events.Add($"moved {signal.BlockId} to {signal.Offset}"));
            _bus.Subscribe<BlockExitedSignal>(signal => _events.Add($"exited {signal.BlockId} {signal.Direction}"));
            _bus.Subscribe<BoardClearedSignal>(_ => _events.Add("cleared"));
        }

        [Test]
        public void Load_ShouldSizeTheBoardSpaceAndAnnounceTheLevel_WhenALevelLoads()
        {
            _board.Load(Level);

            Assert.That(_space.Size, Is.EqualTo(new Vector2Int(5, 6)));
            Assert.That(_events, Is.EqualTo(new[] { "loaded 5x6" }));
            Assert.That(_board.Current.Width, Is.EqualTo(5));
        }

        [Test]
        public void Load_ShouldBuildANewBoard_WhenTheSameLevelLoadsAgain()
        {
            _board.Load(Level);
            BoardState first = _board.Current;
            _board.TryStep(0, Vector2Int.up);

            _board.Load(Level);

            Assert.That(_board.Current, Is.Not.SameAs(first));
            Assert.That(_board.Current.Blocks[0].Offset, Is.EqualTo(Vector2Int.zero));
        }

        [Test]
        public void TryStep_ShouldAnnounceTheMove_WhenTheBlockMoves()
        {
            _board.Load(Level);
            _events.Clear();

            _board.TryStep(0, Vector2Int.up);

            Assert.That(_events, Is.EqualTo(new[] { "moved 0 to (0, 1)" }));
        }

        [Test]
        public void TryStep_ShouldAnnounceNothing_WhenTheBlockIsBlocked()
        {
            _board.Load(Level);
            _events.Clear();

            Assert.That(_board.TryStep(0, Vector2Int.down), Is.EqualTo(StepResult.Blocked));
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void TryStep_ShouldAnnounceTheExitThenTheClear_WhenTheLastBlockLeaves()
        {
            _board.Load(Level);
            _events.Clear();

            _board.TryStep(0, Vector2Int.left);

            Assert.That(_events, Is.EqualTo(new[] { "exited 0 (-1, 0)", "cleared" }));
        }

        [Test]
        public void TryStep_ShouldNotAnnounceTheClear_WhenBlocksRemain()
        {
            _board.Load(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Block(Blue, 3, 3).Door(Left, 0, 1, Red).Build());
            _events.Clear();

            _board.TryStep(0, Vector2Int.left);

            Assert.That(_events, Is.EqualTo(new[] { "exited 0 (-1, 0)" }));
        }
    }
}
