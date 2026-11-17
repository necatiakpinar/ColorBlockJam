using System;
using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    public sealed class DragAllocationTests
    {
        const int WarmUpDrags = 20;
        const int MeasuredDrags = 200;

        [UnityTest]
        public IEnumerator Drag_ShouldAllocateNothing_WhenABlockIsDraggedBackAndForth()
        {
            yield return SceneTestUtility.BootToGameplay();
            IObjectResolver container = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;
            var bus = container.Resolve<SignalBus>();
            BoardState board = container.Resolve<ActiveBoard>().Current;
            var space = container.Resolve<BoardSpace>();
            FindMovableBlock(board, out int id, out Vector2Int step);
            Vector2Int from = board.Blocks[id].GetCell(0);
            Vector3 here = space.CellToWorld(from);
            Vector3 there = space.CellToWorld(from + step);

            int moves = 0;
            bus.Subscribe<BlockMovedSignal>(_ => moves++);

            bus.Fire(new BoardPointerSignal(BoardPointerPhase.Pressed, from, here));
            for (int i = 0; i < WarmUpDrags; i++)
                DragTo(bus, i % 2 == 0 ? there : here, from);
            moves = 0;
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < MeasuredDrags; i++)
                DragTo(bus, i % 2 == 0 ? there : here, from);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            bus.Fire(new BoardPointerSignal(BoardPointerPhase.Released, from, here));

            Assert.That(moves, Is.EqualTo(MeasuredDrags), "Every drag moved the block a cell.");
            Assert.That(allocated, Is.Zero, $"{MeasuredDrags} drags, each a one-cell move, allocated {allocated} B.");
        }

        static void DragTo(SignalBus bus, Vector3 point, Vector2Int cell) =>
            bus.Fire(new BoardPointerSignal(BoardPointerPhase.Dragged, cell, point));

        static void FindMovableBlock(BoardState board, out int id, out Vector2Int step)
        {
            Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
            for (id = 0; id < board.Blocks.Count; id++)
                foreach (Vector2Int direction in directions)
                    if (board.Blocks[id].CanBeGrabbed && board.CanStep(id, direction) == StepResult.Moved)
                    {
                        step = direction;
                        return;
                    }

            Assert.Fail("Level 1 has no block that can move a cell.");
            step = default;
        }
    }
}
