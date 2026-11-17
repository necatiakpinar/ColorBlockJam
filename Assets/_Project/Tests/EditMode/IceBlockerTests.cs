using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Board.Blockers;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Solving;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class IceBlockerTests
    {
        static readonly LevelData Level = new LevelDataBuilder(4, 3)
            .Block(Red, 0, 2).Block(Blue, 3, 2).FrozenBlock(Yellow, 2, 1, 0)
            .Door(Left, 2, 1, Red).Door(Right, 2, 1, Blue).Door(Bottom, 1, 1, Yellow).Build();

        SignalBus _bus;
        ActiveBoard _board;
        readonly List<string> _events = new();

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _board = new ActiveBoard(new BoardSpace(1f), _bus);
            _board.Load(Level);
            _events.Clear();
            _bus.Subscribe<BlockExitedSignal>(signal => _events.Add($"exit {signal.BlockId}"));
            _bus.Subscribe<BlockRemovedSignal>(signal => _events.Add($"removed {signal.BlockId}"));
            _bus.Subscribe<IceCountedDownSignal>(_ => _events.Add("ice"));
            _bus.Subscribe<BlockThawedSignal>(signal => _events.Add($"thawed {signal.BlockId}"));
            _bus.Subscribe<BoardClearedSignal>(_ => _events.Add("cleared"));
        }

        static IceBlocker Ice(BoardBlock block) => block.TryGetBlocker(out IceBlocker ice) ? ice : null;

        static bool Frozen(BoardBlock block) => Ice(block) is { IsFrozen: true };

        [Test]
        public void FrozenBlock_ShouldNotMove_WhileItHasIce()
        {
            BoardBlock yellow = _board.Current.Blocks[2];

            StepResult result = _board.TryStep(2, Vector2Int.down);

            Assert.That(Frozen(yellow), Is.True);
            Assert.That(Ice(yellow).ExitsLeft, Is.EqualTo(2));
            Assert.That(result, Is.EqualTo(StepResult.Blocked));
        }

        [Test]
        public void Exits_ShouldCountTheIceDownAndThawIt_WhenEnoughBlocksLeave()
        {
            _board.TryStep(0, Vector2Int.left);
            int afterOne = Ice(_board.Current.Blocks[2]).ExitsLeft;

            _board.TryStep(1, Vector2Int.right);

            Assert.That(afterOne, Is.EqualTo(1));
            Assert.That(Frozen(_board.Current.Blocks[2]), Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "exit 0", "ice", "exit 1", "ice", "thawed 2" }));
            Assert.That(_board.TryStep(2, Vector2Int.down), Is.EqualTo(StepResult.Exited));
        }

        [Test]
        public void BoosterRemovals_ShouldCountAsExits_WhenTheyTakeBlocks()
        {
            _board.RemoveBlock(0);
            _board.RemoveColor(Blue);

            Assert.That(Frozen(_board.Current.Blocks[2]), Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "removed 0", "ice", "removed 1", "ice", "thawed 2" }));
        }

        [Test]
        public void Thaw_ShouldBreakTheIceAtOnce_WhenTheHammerHitsIt()
        {
            _board.Thaw(2);

            Assert.That(Frozen(_board.Current.Blocks[2]), Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "thawed 2" }));
        }

        [Test]
        public void FrozenBlock_ShouldStillLeaveTheBoardCleared_WhenItGoesLast()
        {
            _board.TryStep(0, Vector2Int.left);
            _board.TryStep(1, Vector2Int.right);

            _board.TryStep(2, Vector2Int.down);

            Assert.That(_events[^1], Is.EqualTo("cleared"));
        }

        [Test]
        public void Drag_ShouldNotPickUpAFrozenBlock_WhenPressed()
        {
            var config = ScriptableObject.CreateInstance<BoardViewConfig>();
            var drag = new BlockDragController(_board, new BoardSpace(1f), config, _bus);
            int grabs = 0;
            _bus.Subscribe<BlockGrabbedSignal>(_ => grabs++);

            _bus.Fire(new BoardPointerSignal(BoardPointerPhase.Pressed, new Vector2Int(1, 0), new Vector3(1.5f, 0f, 0.5f)));

            Assert.That(grabs, Is.Zero);
            drag.Dispose();
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Hammer_ShouldBreakTheIceInsteadOfTheBlock_WhenItHitsAFrozenBlock()
        {
            var catalogs = new TestCatalog();
            var session = new Game.Gameplay.Session.LevelSession(_board, new LevelProvider(catalogs.Create(Level)), _bus);
            var rig = new BoosterRig(_board, session, _bus);
            session.Start(1);

            rig.Service.Activate(Game.Boosters.BoosterType.Hammer);
            _bus.Fire(new BoardPointerSignal(BoardPointerPhase.Pressed, new Vector2Int(1, 0), Vector3.zero));

            BoardBlock yellow = _board.Current.Blocks[2];
            Assert.That(yellow.HasExited, Is.False);
            Assert.That(Frozen(yellow), Is.False);
            Assert.That(rig.Inventory.Count(Game.Boosters.BoosterType.Hammer), Is.Zero);
            rig.Dispose();
            session.Dispose();
            catalogs.Dispose();
        }

        [Test]
        public void Draft_ShouldKeepTheIce_WhenTheLevelRoundTripsThroughTheEditor()
        {
            LevelData copy = Editor.LevelEditing.LevelDraft.From(Level).ToLevel();

            Assert.That(copy.Blocks[2].Blockers.Ice.Exits, Is.EqualTo(2));
            Assert.That(copy.Blocks[0].Blockers.IsEmpty, Is.True);
        }

        [Test]
        public void Solver_ShouldLetTheOthersOutFirst_WhenABlockIsFrozen()
        {
            SolveResult result = new LevelSolver(1000).Solve(Level);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(result.Steps[^1].BlockId, Is.EqualTo(2), "The frozen block leaves last.");
            Assert.That(result.Rounds, Is.EqualTo(2));
        }

        [Test]
        public void Solver_ShouldSayUnsolvable_WhenFrozenBlocksWaitForEachOther()
        {
            LevelData level = new LevelDataBuilder(4, 3)
                .FrozenBlock(Red, 1, 0, 2).FrozenBlock(Blue, 1, 3, 2)
                .Door(Left, 2, 1, Red).Door(Right, 2, 1, Blue).Build();

            Assert.That(new LevelSolver(1000).Solve(level).Status, Is.EqualTo(SolveStatus.Unsolvable));
        }

        [Test]
        public void Validator_ShouldRefuseIce_WhenTheLevelHasTooFewOtherBlocks()
        {
            var rules = ScriptableObject.CreateInstance<LevelRulesConfig>();
            LevelData level = new LevelDataBuilder(4, 3)
                .Block(Red, 0, 2).FrozenBlock(Yellow, 2, 1, 0)
                .Door(Left, 2, 1, Red).Door(Bottom, 1, 1, Yellow).Build();

            IReadOnlyList<LevelIssue> issues = new LevelDefinitionValidator(rules).Validate(level);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Message, Does.Contain("only 1 other blocks"));
            Object.DestroyImmediate(rules);
        }
    }
}
