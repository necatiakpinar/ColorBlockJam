using System;
using NUnit.Framework;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The movement and exit rules (gameplay.md §3, DA-4, DA-5).</summary>
    public sealed class BoardStateTests
    {
        static readonly Vector2Int StepRight = Vector2Int.right;
        static readonly Vector2Int StepLeft = Vector2Int.left;
        static readonly Vector2Int StepUp = Vector2Int.up;
        static readonly Vector2Int StepDown = Vector2Int.down;

        static Vector2Int CellOf(BoardState board, int blockId, int cellIndex = 0) =>
            board.Blocks[blockId].GetCell(cellIndex);

        [Test]
        public void Constructor_ShouldMarkBlockCells_WhenLevelLoads()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 1, 1, 2, 1).Build());

            Assert.That(board.TryGetBlockAt(new Vector2Int(2, 1), out int id), Is.True);
            Assert.That(id, Is.Zero);
            Assert.That(board.TryGetBlockAt(new Vector2Int(3, 1), out _), Is.False);
            Assert.That(board.RemainingBlocks, Is.EqualTo(1));
        }

        [Test]
        public void TryStep_ShouldMoveBlockAndUpdateCells_WhenTargetIsFree()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0, 1, 0).Build());

            StepResult result = board.TryStep(0, StepUp);

            Assert.That(result, Is.EqualTo(StepResult.Moved));
            Assert.That(CellOf(board, 0), Is.EqualTo(new Vector2Int(0, 1)));
            Assert.That(board.TryGetBlockAt(new Vector2Int(0, 0), out _), Is.False);
            Assert.That(board.TryGetBlockAt(new Vector2Int(1, 1), out _), Is.True);
        }

        [Test]
        public void TryStep_ShouldStopAtWall_WhenNoDoorIsThere()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Build());

            Assert.That(board.TryStep(0, StepLeft), Is.EqualTo(StepResult.Blocked));
            Assert.That(CellOf(board, 0), Is.EqualTo(Vector2Int.zero));
        }

        [Test]
        public void TryStep_ShouldStopAtAnotherBlock_WhenItIsInTheWay()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Block(Blue, 1, 0).Build());

            Assert.That(board.TryStep(0, StepRight), Is.EqualTo(StepResult.Blocked));
        }

        [Test]
        public void TryStep_ShouldSlideAcrossItsOwnCells_WhenMovingAlongItsLength()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0, 1, 0).Build());

            Assert.That(board.TryStep(0, StepRight), Is.EqualTo(StepResult.Moved));
            Assert.That(CellOf(board, 0, 1), Is.EqualTo(new Vector2Int(2, 0)));
        }

        [Test]
        public void TryStep_ShouldExit_WhenAMatchingDoorCoversTheBlock()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4)
                .Block(Red, 1, 0, 2, 0)
                .Door(Bottom, 1, 2, Red)
                .Build());

            Assert.That(board.TryStep(0, StepDown), Is.EqualTo(StepResult.Exited));
            Assert.That(board.Blocks[0].HasExited, Is.True);
            Assert.That(board.TryGetBlockAt(new Vector2Int(1, 0), out _), Is.False);
        }

        [Test]
        public void TryStep_ShouldExitThroughAWiderDoor_WhenTheBlockFitsInside()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 3, 1).Door(Right, 0, 4, Red).Build());

            Assert.That(board.TryStep(0, StepRight), Is.EqualTo(StepResult.Exited));
        }

        [Test]
        public void TryStep_ShouldExitThroughTheTopWall_WhenAMatchingDoorIsAbove()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 2, 3).Door(Top, 2, 1, Red).Build());

            Assert.That(board.TryStep(0, StepUp), Is.EqualTo(StepResult.Exited));
        }

        [Test]
        public void TryStep_ShouldBeBlocked_WhenTheDoorColourDiffers()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Door(Bottom, 0, 1, Blue).Build());

            Assert.That(board.TryStep(0, StepDown), Is.EqualTo(StepResult.Blocked));
        }

        [Test]
        public void TryStep_ShouldBeBlocked_WhenTheDoorIsNarrowerThanTheBlock()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0, 1, 0).Door(Bottom, 0, 1, Red).Build());

            Assert.That(board.TryStep(0, StepDown), Is.EqualTo(StepResult.Blocked));
        }

        [Test]
        public void TryStep_ShouldBeBlocked_WhenTheDoorOnlyPartlyCoversTheBlock()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0, 1, 0).Door(Bottom, 1, 2, Red).Build());

            Assert.That(board.TryStep(0, StepDown), Is.EqualTo(StepResult.Blocked));
        }

        [Test]
        public void TryStep_ShouldBeBlocked_WhenTheDoorIsOnAnotherWall()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build());

            Assert.That(board.TryStep(0, StepDown), Is.EqualTo(StepResult.Blocked));
        }

        [Test]
        public void TryStep_ShouldBeBlocked_WhenAnotherBlockStandsInTheWayOut()
        {
            // L-shaped red: its top row touches the right wall. Blue sits two cells ahead of the bottom
            // cell, so the one-cell step itself is free but the straight path out is not.
            var board = new BoardState(new LevelDataBuilder(4, 4)
                .Block(Red, 1, 0, 1, 1, 2, 1, 3, 1)
                .Block(Blue, 3, 0)
                .Door(Right, 0, 2, Red)
                .Build());

            Assert.That(board.TryStep(0, StepRight), Is.EqualTo(StepResult.Blocked));
            Assert.That(board.Blocks[0].HasExited, Is.False);
        }

        [Test]
        public void TryStep_ShouldExit_WhenOnlyThePartNearestTheWallTouchesIt()
        {
            // Same L shape with the path clear: the bottom row slides out behind the top row.
            var board = new BoardState(new LevelDataBuilder(4, 4)
                .Block(Red, 2, 0, 2, 1, 3, 1)
                .Door(Right, 0, 2, Red)
                .Build());

            Assert.That(board.TryStep(0, StepRight), Is.EqualTo(StepResult.Exited));
        }

        [Test]
        public void CanStep_ShouldPredictTheStepWithoutChangingTheBoard_WhenAsked()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4)
                .Block(Red, 0, 0)
                .Block(Blue, 1, 0)
                .Door(Left, 0, 1, Red)
                .Build());

            Assert.That(board.CanStep(0, StepUp), Is.EqualTo(StepResult.Moved));
            Assert.That(board.CanStep(0, StepRight), Is.EqualTo(StepResult.Blocked));
            Assert.That(board.CanStep(0, StepLeft), Is.EqualTo(StepResult.Exited));
            Assert.That(CellOf(board, 0), Is.EqualTo(Vector2Int.zero));
            Assert.That(board.Blocks[0].HasExited, Is.False);
        }

        [Test]
        public void IsCleared_ShouldBecomeTrue_WhenTheLastBlockExits()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4)
                .Block(Red, 0, 0)
                .Block(Blue, 3, 3)
                .Door(Left, 0, 1, Red)
                .Door(Right, 3, 1, Blue)
                .Build());

            board.TryStep(0, StepLeft);
            Assert.That(board.IsCleared, Is.False);
            board.TryStep(1, StepRight);

            Assert.That(board.IsCleared, Is.True);
            Assert.That(board.RemainingBlocks, Is.Zero);
        }

        [Test]
        public void NewBoard_ShouldStartFromTheLevel_WhenAnEarlierAttemptMovedBlocks()
        {
            LevelData level = new LevelDataBuilder(4, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build();
            var firstAttempt = new BoardState(level);
            firstAttempt.TryStep(0, StepUp);
            firstAttempt.TryStep(0, StepUp);

            var restart = new BoardState(level);

            Assert.That(CellOf(restart, 0), Is.EqualTo(Vector2Int.zero));
            Assert.That(restart.TryStep(0, StepLeft), Is.EqualTo(StepResult.Exited));
            Assert.That(firstAttempt.Blocks[0].HasExited, Is.False);
        }

        [Test]
        public void TryStep_ShouldThrow_WhenDirectionIsNotOneCell()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Build());

            Assert.Throws<ArgumentException>(() => board.TryStep(0, new Vector2Int(1, 1)));
            Assert.Throws<ArgumentException>(() => board.TryStep(0, new Vector2Int(2, 0)));
        }

        [Test]
        public void TryStep_ShouldThrow_WhenTheBlockHasAlreadyExited()
        {
            var board = new BoardState(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build());
            board.TryStep(0, StepLeft);

            Assert.Throws<InvalidOperationException>(() => board.TryStep(0, StepRight));
        }

        [Test]
        public void Constructor_ShouldThrow_WhenBlocksOverlap()
        {
            LevelData invalid = new LevelDataBuilder(4, 4).Block(Red, 0, 0).Block(Blue, 0, 0).Build();

            Assert.Throws<ArgumentException>(() => _ = new BoardState(invalid));
        }

        [Test]
        public void TryStep_ShouldNotAllocate_WhenMovingOrBlocked()
        {
            // Red shuttles through open cells and pushes a doorless wall; blue pushes a door of the wrong
            // colour; the green L fits its door but pink stands in its way out, so the path check runs.
            var board = new BoardState(new LevelDataBuilder(6, 6)
                .Block(Red, 0, 3, 1, 3, 1, 4)
                .Block(Blue, 4, 5)
                .Block(Green, 3, 0, 3, 1, 4, 1, 5, 1)
                .Block(Pink, 5, 0)
                .Door(Top, 4, 2, Red)
                .Door(Right, 0, 2, Green)
                .Build());
            void Steps()
            {
                for (int i = 0; i < 1000; i++)
                {
                    board.TryStep(0, StepRight);
                    board.TryStep(0, StepLeft);
                    board.TryStep(0, StepLeft);
                    board.TryStep(1, StepUp);
                    board.TryStep(2, StepRight);
                }
            }
            Steps();

            Assert.That(Steps, Is.Not.AllocatingGCMemory());
            Assert.That(board.TryStep(2, StepRight), Is.EqualTo(StepResult.Blocked), "The path case must stay blocked.");
        }
    }
}
