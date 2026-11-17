using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>Boosters taking blocks and cells off the board (boosters.md §2, DA-36).</summary>
    public sealed class BoardRemovalTests
    {
        // A red bar of three over a blue cell and a red cell; the doors let each colour out.
        static readonly LevelData Level = new LevelDataBuilder(4, 4)
            .Block(Red, 0, 3, 1, 3, 2, 3).Block(Blue, 0, 0).Block(Red, 3, 0)
            .Door(Top, 0, 3, Red).Door(Bottom, 0, 1, Blue).Door(Right, 0, 1, Red).Build();

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
            _bus.Subscribe<BlockRemovedSignal>(signal => _events.Add($"removed {signal.BlockId} {signal.Kind} #{signal.Order}"));
            _bus.Subscribe<BlockCellRemovedSignal>(signal => _events.Add($"cell {signal.BlockId} {signal.Cell}"));
            _bus.Subscribe<BoardClearedSignal>(_ => _events.Add("cleared"));
        }

        [Test]
        public void RemoveBlock_ShouldFreeItsCellsAndCountItGone_WhenSmashed()
        {
            _board.RemoveBlock(0);

            BoardState state = _board.Current;
            Assert.That(state.Blocks[0].HasExited, Is.True);
            Assert.That(state.RemainingBlocks, Is.EqualTo(2));
            Assert.That(state.TryGetBlockAt(new Vector2Int(1, 3), out _), Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "removed 0 Smashed #0" }));
        }

        [Test]
        public void RemoveCell_ShouldKeepTheRestOfTheBlock_WhenItHasOtherCells()
        {
            _board.RemoveCell(0, new Vector2Int(1, 3));

            BoardBlock bar = _board.Current.Blocks[0];
            Assert.That(bar.HasExited, Is.False);
            Assert.That(bar.StartCells, Is.EqualTo(new[] { new Vector2Int(0, 3), new Vector2Int(2, 3) }));
            Assert.That(_board.Current.TryGetBlockAt(new Vector2Int(1, 3), out _), Is.False);
            Assert.That(_board.Current.TryGetBlockAt(new Vector2Int(2, 3), out int owner) && owner == 0, Is.True);
            Assert.That(_events, Is.EqualTo(new[] { "cell 0 (1, 3)" }));
        }

        [Test]
        public void RemoveCell_ShouldRemoveTheBlock_WhenItWasItsLastCell()
        {
            _board.RemoveCell(1, new Vector2Int(0, 0));

            Assert.That(_board.Current.Blocks[1].HasExited, Is.True);
            Assert.That(_events, Is.EqualTo(new[] { "removed 1 Blasted #0" }));
        }

        [Test]
        public void RemoveCell_ShouldThrow_WhenTheBlockHasNoSuchCell()
        {
            Assert.That(() => _board.RemoveCell(0, new Vector2Int(3, 3)), Throws.ArgumentException);
        }

        [Test]
        public void RemoveColor_ShouldTakeEveryBlockOfThatColourInOrder_WhenVacuumed()
        {
            int removed = _board.RemoveColor(Red);

            Assert.That(removed, Is.EqualTo(2));
            Assert.That(_board.Current.RemainingBlocks, Is.EqualTo(1));
            Assert.That(_events, Is.EqualTo(new[] { "removed 0 Vacuumed #0", "removed 2 Vacuumed #1" }));
        }

        [Test]
        public void Removal_ShouldClearTheBoardOnce_WhenTheLastBlocksGo()
        {
            _board.RemoveBlock(1);

            _board.RemoveColor(Red);

            Assert.That(_board.Current.IsCleared, Is.True);
            Assert.That(_events, Has.Exactly(1).EqualTo("cleared"));
            Assert.That(_events[^1], Is.EqualTo("cleared"));
        }

        [Test]
        public void BlastedBlock_ShouldStillMoveAndExitAsOne_WhenItFellApart()
        {
            // The bar loses its middle: its two ends keep moving together and leave through the top door.
            _board.RemoveCell(0, new Vector2Int(1, 3));

            StepResult result = _board.TryStep(0, Vector2Int.up);

            Assert.That(result, Is.EqualTo(StepResult.Exited));
        }

        [Test]
        public void RemoveBlock_ShouldThrow_WhenTheBlockIsAlreadyGone()
        {
            _board.RemoveBlock(1);

            Assert.That(() => _board.RemoveBlock(1), Throws.InvalidOperationException);
        }
    }
}
