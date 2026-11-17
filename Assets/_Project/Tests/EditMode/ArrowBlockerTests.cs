using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Board.Blockers;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Blockers;
using ColorBlockJam.Game.Levels.Solving;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>Arrow blocks: they move, and leave, only along their axis (T-35, DA-39, blockers.md).</summary>
    public sealed class ArrowBlockerTests
    {
        static BlockerSet Arrow(ArrowAxis axis) => new(null, new ArrowBlockerData(axis));

        // A horizontal red block in the middle with doors on the left and on top; only the left one can take it.
        static LevelData Level() => new LevelDataBuilder(4, 4)
            .Blocked(Red, Arrow(ArrowAxis.Horizontal), 1, 1, 2, 1)
            .Door(Left, 1, 1, Red).Door(Top, 1, 2, Red).Build();

        SignalBus _bus;
        BoardSpace _space;
        ActiveBoard _board;

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _space = new BoardSpace(1f);
            _board = new ActiveBoard(_space, _bus);
            _board.Load(Level());
        }

        [Test]
        public void HorizontalArrow_ShouldMoveSideways_WhenPushedAlongItsAxis()
        {
            Assert.That(_board.TryStep(0, Vector2Int.right), Is.EqualTo(StepResult.Moved));
        }

        [Test]
        public void HorizontalArrow_ShouldBeBlocked_WhenPushedAcrossItsAxis()
        {
            Assert.That(_board.TryStep(0, Vector2Int.up), Is.EqualTo(StepResult.Blocked));
            Assert.That(_board.TryStep(0, Vector2Int.down), Is.EqualTo(StepResult.Blocked));
        }

        [Test]
        public void HorizontalArrow_ShouldLeaveOnlyThroughASideDoor_WhenItReachesOne()
        {
            _board.TryStep(0, Vector2Int.left);

            Assert.That(_board.TryStep(0, Vector2Int.left), Is.EqualTo(StepResult.Exited));
        }

        [Test]
        public void ArrowBlock_ShouldStillBePickedUp_WhenItHasNoIce()
        {
            Assert.That(_board.Current.Blocks[0].CanBeGrabbed, Is.True);
        }

        [Test]
        public void Drag_ShouldNotGiveAcrossTheAxis_WhenTheFingerPullsThatWay()
        {
            var config = ScriptableObject.CreateInstance<BoardViewConfig>();
            var drag = new BlockDragController(_board, _space, config, _bus);
            var drawn = new List<Vector2>();
            _bus.Subscribe<BlockDraggedSignal>(signal => drawn.Add(signal.Offset));

            _bus.Fire(new BoardPointerSignal(BoardPointerPhase.Pressed, new Vector2Int(1, 1), _space.CellToWorld(new Vector2Int(1, 1))));
            _bus.Fire(new BoardPointerSignal(BoardPointerPhase.Dragged, new Vector2Int(1, 2), _space.CellToWorld(new Vector2Int(1, 2))));

            Assert.That(drawn[^1].y, Is.EqualTo(0f).Within(1e-4f), "The block stays on its row.");
            Assert.That(_board.Current.Blocks[0].Offset, Is.EqualTo(Vector2Int.zero));
            drag.Dispose();
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Solver_ShouldFindNoWayOut_WhenTheOnlyDoorIsAcrossTheAxis()
        {
            LevelData level = new LevelDataBuilder(4, 4)
                .Blocked(Red, Arrow(ArrowAxis.Vertical), 1, 1).Door(Left, 1, 1, Red).Build();

            Assert.That(new LevelSolver(1000).Solve(level).Status, Is.EqualTo(SolveStatus.Unsolvable));
        }

        [Test]
        public void Validator_ShouldSayTheArrowBlockCanNeverLeave_WhenNoDoorLiesAlongItsAxis()
        {
            var rules = ScriptableObject.CreateInstance<LevelRulesConfig>();
            LevelData level = new LevelDataBuilder(4, 4)
                .Blocked(Red, Arrow(ArrowAxis.Vertical), 1, 1).Door(Left, 1, 1, Red).Build();

            IReadOnlyList<LevelIssue> issues = new LevelDefinitionValidator(rules).Validate(level);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Message, Does.Contain("moves only vertically"));
            Object.DestroyImmediate(rules);
        }

        [Test]
        public void SplitPiece_ShouldKeepTheArrow_WhenTheRocketBreaksTheBlock()
        {
            _board.Load(new LevelDataBuilder(5, 4).Blocked(Red, Arrow(ArrowAxis.Horizontal), 1, 1, 2, 1, 3, 1)
                .Door(Left, 1, 1, Red).Build());

            _board.RemoveCell(0, new Vector2Int(2, 1));

            Assert.That(_board.Current.Blocks[1].TryGetBlocker(out ArrowBlocker arrow), Is.True);
            Assert.That(arrow.Axis, Is.EqualTo(ArrowAxis.Horizontal));
        }

        [Test]
        public void Draft_ShouldKeepTheArrow_WhenTheLevelRoundTripsThroughTheEditor()
        {
            LevelData copy = ColorBlockJam.Editor.LevelEditing.LevelDraft.From(Level()).ToLevel();

            Assert.That(copy.Blocks[0].Blockers.Arrow.Axis, Is.EqualTo(ArrowAxis.Horizontal));
        }
    }
}
