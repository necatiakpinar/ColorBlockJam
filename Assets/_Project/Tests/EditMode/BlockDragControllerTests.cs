using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class BlockDragControllerTests
    {
        SignalBus _bus;
        BoardSpace _space;
        BoardViewConfig _config;
        ActiveBoard _board;
        BlockDragController _controller;
        readonly List<BlockDraggedSignal> _drags = new();
        readonly List<BlockDroppedSignal> _drops = new();
        readonly List<BlockExitedSignal> _exits = new();
        int _clears;

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _space = new BoardSpace(1f);
            _board = new ActiveBoard(_space, _bus);
            _config = CreateConfig(holdGive: 0f);
            _controller = new BlockDragController(_board, _space, _config, _bus);
            _drags.Clear();
            _drops.Clear();
            _exits.Clear();
            _clears = 0;
            _bus.Subscribe<BlockDraggedSignal>(_drags.Add);
            _bus.Subscribe<BlockDroppedSignal>(_drops.Add);
            _bus.Subscribe<BlockExitedSignal>(_exits.Add);
            _bus.Subscribe<BoardClearedSignal>(_ => _clears++);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
            Object.DestroyImmediate(_config);
        }

        static BoardViewConfig CreateConfig(float holdGive)
        {
            var config = ScriptableObject.CreateInstance<BoardViewConfig>();
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_holdGive").floatValue = holdGive;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        void Finger(BoardPointerPhase phase, float x, float y)
        {
            Vector3 world = _space.CellToWorld(Vector2Int.zero) + new Vector3(x, 0f, y) * _space.CellSize;
            var cell = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
            _bus.Fire(new BoardPointerSignal(phase, cell, world));
        }

        Vector2Int CellOf(int blockId) => _board.Current.Blocks[blockId].Offset;

        Vector2 LastDrag => _drags[_drags.Count - 1].Offset;

        static void AssertVector(Vector2 actual, float x, float y)
        {
            Assert.That(actual.x, NUnit.Framework.Is.EqualTo(x).Within(1e-3f), "x");
            Assert.That(actual.y, NUnit.Framework.Is.EqualTo(y).Within(1e-3f), "y");
        }

        [Test]
        public void Drag_ShouldFollowTheFingerBetweenCells_WhenTheWayIsFree()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0, 1, 0).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 0, 0.3f);

            AssertVector(LastDrag, 0f, 0.3f);
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(Vector2Int.zero), "Not past the middle: still its cell.");
        }

        [Test]
        public void Drag_ShouldCommitTheNextCell_WhenTheBlockPassesItsMiddle()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0, 1, 0).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 0, 0.7f);

            AssertVector(LastDrag, 0f, 0.7f);
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(new Vector2Int(0, 1)));
        }

        [Test]
        public void Drag_ShouldCrossSeveralFreeCellsInOneEvent_WhenTheFingerMovesFast()
        {
            _board.Load(new LevelDataBuilder(6, 6).Block(Red, 0, 0).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 4.2f, 0);

            AssertVector(LastDrag, 4.2f, 0f);
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(new Vector2Int(4, 0)));
        }

        [Test]
        public void Drag_ShouldStopOnTheCellEdge_WhenAFastFingerPassesAnObstacle()
        {
            _board.Load(new LevelDataBuilder(6, 6).Block(Red, 0, 0).Block(Blue, 3, 0).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 5f, 0);

            AssertVector(LastDrag, 2f, 0f);
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(new Vector2Int(2, 0)));
        }

        [Test]
        public void Release_ShouldSnapToTheNearestCell_WhenTheBlockIsBetweenCells()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 1.4f, 0);

            Finger(BoardPointerPhase.Released, 1.4f, 0);

            Assert.That(_drops.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(_drops[0].Offset, NUnit.Framework.Is.EqualTo(new Vector2Int(1, 0)));
        }

        [Test]
        public void Drag_ShouldTurnTheCorner_WhenTheMainAxisIsBlockedAndTheFingerAlsoMovedTheOtherWay()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Block(Blue, 1, 0).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 2f, 1f);

            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(new Vector2Int(2, 1)));
        }

        [Test]
        public void Drag_ShouldMoveDiagonally_WhenTheWayIsFree()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 1.3f, 0.8f);

            AssertVector(LastDrag, 1.3f, 0.8f);
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(new Vector2Int(1, 1)), "The board holds the nearest cell.");
        }

        [Test]
        public void Drag_ShouldSlideAlongAnObstacle_WhenTheFingerPushesIntoItAtAnAngle()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Block(Blue, 1, 0).Block(Green, 1, 1).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 1.5f, 0.6f);

            AssertVector(LastDrag, 0f, 0.6f);
        }

        [Test]
        public void Drag_ShouldEaseIntoAOneCellGap_WhenPushedThroughItSlightlyOffTheCellLine()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Purple, 0, 2).Block(Red, 2, 2).Block(Orange, 1, 0).Build());
            Finger(BoardPointerPhase.Pressed, 1, 0);
            Finger(BoardPointerPhase.Dragged, 1.2f, 0);

            Finger(BoardPointerPhase.Dragged, 1.2f, 3f);

            Assert.That(CellOf(2), NUnit.Framework.Is.EqualTo(new Vector2Int(0, 3)));
            AssertVector(LastDrag, 0f, 3f);
        }

        [Test]
        public void Drag_ShouldNotJitter_WhenPushedSlowlyIntoAOneCellGapSlightlyOffTheCellLine()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Purple, 0, 2).Block(Red, 2, 2).Block(Orange, 1, 0).Build());
            Finger(BoardPointerPhase.Pressed, 1, 0);
            Finger(BoardPointerPhase.Dragged, 1.2f, 0);

            float previousX = float.MaxValue;
            for (float y = 0.9f; y <= 1.4f; y += 0.02f)
            {
                Finger(BoardPointerPhase.Dragged, 1.2f, y);
                Assert.That(LastDrag.x, NUnit.Framework.Is.LessThanOrEqualTo(previousX + 1e-4f), $"x went back at y {y}");
                previousX = LastDrag.x;
            }

            Assert.That(LastDrag.x, NUnit.Framework.Is.EqualTo(0f).Within(1e-3f), "It ended up on the gap's line.");
            Assert.That(LastDrag.y, NUnit.Framework.Is.GreaterThan(1f), "It went into the gap.");
        }

        void LoadCorridorBoard() => _board.Load(new LevelDataBuilder(5, 6)
            .Block(Yellow, 0, 4, 1, 4, 0, 5, 1, 5)
            .Block(Purple, 2, 1, 2, 2)
            .Block(Green, 0, 1, 1, 1)
            .Block(Red, 2, 0, 3, 0, 3, 1)
            .Block(Blue, 4, 1, 4, 2, 4, 3)
            .Block(Orange, 3, 4, 3, 5, 4, 5)
            .Build());

        [Test]
        public void Drag_ShouldTurnIntoASideCorridor_WhenItSlippedTowardAnotherGapFirst()
        {
            LoadCorridorBoard();
            Finger(BoardPointerPhase.Pressed, 2, 1.5f);
            for (float y = 1.5f; y <= 2.7f; y += 0.1f)
                Finger(BoardPointerPhase.Dragged, 2.2f, y);
            Finger(BoardPointerPhase.Dragged, 2f, 2.3f);

            for (float x = 2f; x <= 3.2f; x += 0.1f)
                Finger(BoardPointerPhase.Dragged, x, 2.3f);

            Assert.That(CellOf(1), NUnit.Framework.Is.EqualTo(new Vector2Int(1, 1)), "In column 3, one row up.");
        }

        [Test]
        public void Drag_ShouldEnterTheGap_WhenTheFingerWobblesSidewaysAtItsMouth()
        {
            LoadCorridorBoard();
            Finger(BoardPointerPhase.Pressed, 2, 1.5f);
            float[] wobble = { 0.01f, -0.01f, 0.005f, -0.008f };
            int i = 0;
            for (float y = 1.5f; y <= 4.5f; y += 0.04f)
                Finger(BoardPointerPhase.Dragged, 2f + wobble[i++ % wobble.Length], y);

            Assert.That(CellOf(1), NUnit.Framework.Is.EqualTo(new Vector2Int(0, 3)), "Up through the gap.");
        }

        [Test]
        public void Drag_ShouldNotSlipSideways_WhenNoCellLineOpensTheWay()
        {
            _board.Load(new LevelDataBuilder(5, 5)
                .Block(Purple, 0, 2).Block(Blue, 1, 2).Block(Red, 2, 2).Block(Orange, 1, 0).Build());
            Finger(BoardPointerPhase.Pressed, 1, 0);
            Finger(BoardPointerPhase.Dragged, 1.2f, 0);

            Finger(BoardPointerPhase.Dragged, 1.2f, 3f);

            AssertVector(LastDrag, 0.2f, 1f);
        }

        [Test]
        public void Drag_ShouldNotSqueezeBetweenRows_WhenOnlyOneOfTheTwoRowsItSpansIsFree()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Block(Blue, 1, 0).Build());
            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 0, 0.4f);

            Finger(BoardPointerPhase.Dragged, 2f, 0.4f);

            AssertVector(LastDrag, 0f, 0.4f);
        }

        [Test]
        public void Drag_ShouldLeanTowardTheFingerButNotCommit_WhenTheBlockHasNoRoom()
        {
            BoardViewConfig leaning = CreateConfig(holdGive: 0.12f);
            var bus = new SignalBus();
            var space = new BoardSpace(1f);
            var board = new ActiveBoard(space, bus);
            using var controller = new BlockDragController(board, space, leaning, bus);
            var drags = new List<BlockDraggedSignal>();
            var drops = new List<BlockDroppedSignal>();
            bus.Subscribe<BlockDraggedSignal>(drags.Add);
            bus.Subscribe<BlockDroppedSignal>(drops.Add);
            board.Load(new LevelDataBuilder(3, 3).Block(Red, 1, 1).Block(Blue, 2, 1).Build());
            Vector3 centre = space.CellToWorld(new Vector2Int(1, 1));

            bus.Fire(new BoardPointerSignal(BoardPointerPhase.Pressed, new Vector2Int(1, 1), centre));
            bus.Fire(new BoardPointerSignal(BoardPointerPhase.Dragged, new Vector2Int(2, 1), centre + Vector3.right * 0.8f));
            bus.Fire(new BoardPointerSignal(BoardPointerPhase.Released, new Vector2Int(2, 1), centre + Vector3.right * 0.8f));

            AssertVector(drags[0].Offset, 0.12f, 0f);
            Assert.That(drops[0].Offset, NUnit.Framework.Is.EqualTo(Vector2Int.zero), "It snaps back to its own cell.");
            Object.DestroyImmediate(leaning);
        }

        [Test]
        public void Press_ShouldAnnounceTheGrab_WhenItHitsABlock()
        {
            _board.Load(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Block(Blue, 2, 2).Build());
            var grabs = new List<BlockGrabbedSignal>();
            _bus.Subscribe<BlockGrabbedSignal>(grabs.Add);

            Finger(BoardPointerPhase.Pressed, 2, 2);

            Assert.That(grabs.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(grabs[0].BlockId, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void Drag_ShouldStayWithTheFinger_WhenItSlippedIntoAGapAndCameOutAgain()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Purple, 0, 2).Block(Red, 2, 2).Block(Orange, 1, 0).Build());
            Finger(BoardPointerPhase.Pressed, 1, 0);
            Finger(BoardPointerPhase.Dragged, 1.2f, 0);
            Finger(BoardPointerPhase.Dragged, 1.2f, 3f);

            Finger(BoardPointerPhase.Dragged, 1.2f, 4f);

            AssertVector(LastDrag, 0.2f, 4f);
        }

        [Test]
        public void Grab_ShouldPickTheNearestBlock_WhenThePressMissesItByLittle()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Block(Blue, 3, 3).Build());
            var grabbed = new List<BlockGrabbedSignal>();
            _bus.Subscribe<BlockGrabbedSignal>(grabbed.Add);

            Finger(BoardPointerPhase.Pressed, 0.75f, 0f);

            Assert.That(grabbed.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(grabbed[0].BlockId, NUnit.Framework.Is.Zero);
        }

        [Test]
        public void Grab_ShouldPickNothing_WhenThePressIsBeyondTheTouchTolerance()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            var grabbed = new List<BlockGrabbedSignal>();
            _bus.Subscribe<BlockGrabbedSignal>(grabbed.Add);

            Finger(BoardPointerPhase.Pressed, 0.95f, 0f);

            Assert.That(grabbed, NUnit.Framework.Is.Empty);
        }

        [Test]
        public void Pause_ShouldDropTheHeldBlockOntoItsCell_WhenTheLevelPausesMidDrag()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 1.3f, 0);

            _bus.Fire(new LevelPausedSignal());

            Assert.That(_drops.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(_drops[0].Offset, NUnit.Framework.Is.EqualTo(new Vector2Int(1, 0)), "Onto the cell the board has.");
        }

        [Test]
        public void Cancel_ShouldDropTheHeldBlockOntoItsCellAndTakeTheNextPress_WhenTheTouchIsLost()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 1.3f, 0);

            Finger(BoardPointerPhase.Cancelled, 1.3f, 0);
            Finger(BoardPointerPhase.Pressed, 1, 0);
            Finger(BoardPointerPhase.Dragged, 3f, 0);

            Assert.That(_drops[0].Offset, NUnit.Framework.Is.EqualTo(new Vector2Int(1, 0)), "Onto the cell the board has.");
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(new Vector2Int(3, 0)), "The next press drags again.");
        }

        [Test]
        public void Win_ShouldForgetTheExitedBlockWithoutDroppingIt_WhenTheLastExitCompletesTheLevel()
        {
            _board.Load(new LevelDataBuilder(4, 4).Block(Red, 0, 1).Door(Left, 1, 1, Red).Build());
            _bus.Subscribe<BoardClearedSignal>(_ => _bus.Fire(new LevelCompletedSignal(1)));
            Finger(BoardPointerPhase.Pressed, 0, 1);

            Assert.DoesNotThrow(() => Finger(BoardPointerPhase.Dragged, -1f, 1));

            Assert.That(_exits.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(_drops, NUnit.Framework.Is.Empty, "An exited block is not dropped.");
        }

        [Test]
        public void Input_ShouldBeIgnored_UntilTheLevelResumes()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            _bus.Fire(new LevelPausedSignal());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 2f, 0);
            int dragsWhilePaused = _drags.Count;
            _bus.Fire(new LevelResumedSignal());
            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 2f, 0);

            Assert.That(dragsWhilePaused, NUnit.Framework.Is.Zero);
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(new Vector2Int(2, 0)), "Moves again once resumed.");
        }

        [Test]
        public void Input_ShouldBeIgnored_WhenTheLevelIsLost()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            _bus.Fire(new LevelFailedSignal(1));

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, 2f, 0);

            Assert.That(_drags, NUnit.Framework.Is.Empty);
            Assert.That(CellOf(0), NUnit.Framework.Is.EqualTo(Vector2Int.zero));
        }

        [Test]
        public void Drag_ShouldDoNothing_WhenThePressMissedEveryBlock()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());

            Finger(BoardPointerPhase.Pressed, 3, 3);
            Finger(BoardPointerPhase.Dragged, 0, 3);
            Finger(BoardPointerPhase.Released, 0, 3);

            Assert.That(_drags, NUnit.Framework.Is.Empty);
            Assert.That(_drops, NUnit.Framework.Is.Empty);
        }

        [Test]
        public void Drag_ShouldExit_WhenTheBlockIsPushedJustIntoItsDoor()
        {
            _board.Load(new LevelDataBuilder(4, 4).Block(Red, 0, 1).Block(Blue, 3, 3).Door(Left, 1, 1, Red).Build());
            Finger(BoardPointerPhase.Pressed, 0, 1);

            Finger(BoardPointerPhase.Dragged, -0.1f, 1);
            int afterSmallPush = _exits.Count;
            Finger(BoardPointerPhase.Dragged, -0.2f, 1);

            Assert.That(afterSmallPush, NUnit.Framework.Is.Zero, "A push shorter than ExitPush keeps it in.");
            Assert.That(_exits.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(_exits[0].Offset, NUnit.Framework.Is.EqualTo(Vector2Int.zero), "It leaves from its cell by the door.");
        }

        [Test]
        public void Drag_ShouldExitAndClearTheBoard_WhenTheLastBlockIsPushedIntoItsDoor()
        {
            _board.Load(new LevelDataBuilder(4, 4).Block(Red, 1, 1).Door(Left, 1, 1, Red).Build());

            Finger(BoardPointerPhase.Pressed, 1, 1);
            Finger(BoardPointerPhase.Dragged, -0.6f, 1);
            Finger(BoardPointerPhase.Released, -0.6f, 1);

            Assert.That(_exits.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(_exits[0].Direction, NUnit.Framework.Is.EqualTo(Vector2Int.left));
            Assert.That(_clears, NUnit.Framework.Is.EqualTo(1));
            Assert.That(_drops, NUnit.Framework.Is.Empty, "An exited block is not dropped.");
        }

        [Test]
        public void Drag_ShouldStopAtTheWall_WhenTheDoorIsTheWrongColour()
        {
            _board.Load(new LevelDataBuilder(4, 4).Block(Red, 1, 1).Door(Left, 1, 1, Blue).Build());

            Finger(BoardPointerPhase.Pressed, 1, 1);
            Finger(BoardPointerPhase.Dragged, -2f, 1);

            AssertVector(LastDrag, -1f, 0f);
            Assert.That(_exits, NUnit.Framework.Is.Empty);
        }

        [Test]
        public void Drag_ShouldNotClearTheBoard_WhenOtherBlocksRemain()
        {
            _board.Load(new LevelDataBuilder(4, 4).Block(Red, 0, 0).Block(Blue, 3, 3).Door(Left, 0, 1, Red).Build());

            Finger(BoardPointerPhase.Pressed, 0, 0);
            Finger(BoardPointerPhase.Dragged, -1f, 0);

            Assert.That(_exits.Count, NUnit.Framework.Is.EqualTo(1));
            Assert.That(_clears, NUnit.Framework.Is.Zero);
        }

        [Test]
        public void Load_ShouldDropTheCurrentDrag_WhenANewBoardStarts()
        {
            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            Finger(BoardPointerPhase.Pressed, 0, 0);

            _board.Load(new LevelDataBuilder(5, 5).Block(Red, 0, 0).Build());
            Finger(BoardPointerPhase.Dragged, 3, 0);

            Assert.That(_drags, NUnit.Framework.Is.Empty);
        }

        [Test]
        public void Drag_ShouldNotAllocate_WhenAFingerMovesABlockBackAndForth()
        {
            var bus = new SignalBus();
            var space = new BoardSpace(1f);
            var board = new ActiveBoard(space, bus);
            using var controller = new BlockDragController(board, space, _config, bus);
            board.Load(new LevelDataBuilder(6, 6).Block(Red, 0, 0, 1, 0).Block(Blue, 5, 5).Build());
            Vector3 origin = space.CellToWorld(Vector2Int.zero);
            var press = new BoardPointerSignal(BoardPointerPhase.Pressed, Vector2Int.zero, origin);
            var far = new BoardPointerSignal(BoardPointerPhase.Dragged, new Vector2Int(4, 3), origin + new Vector3(3.7f, 0f, 2.6f));
            var back = new BoardPointerSignal(BoardPointerPhase.Dragged, Vector2Int.zero, origin);
            bus.Fire(press);
            void Drags()
            {
                for (int i = 0; i < 500; i++)
                {
                    bus.Fire(far);
                    bus.Fire(back);
                }
            }
            Drags();

            Assert.That(Drags, Is.Not.AllocatingGCMemory());
        }
    }
}
