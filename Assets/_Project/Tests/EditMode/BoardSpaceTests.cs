using System;
using NUnit.Framework;
using ColorBlockJam.Game.Gameplay.Board;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class BoardSpaceTests
    {
        static BoardSpace Create(float cellSize, int width, int height)
        {
            var space = new BoardSpace(cellSize);
            space.SetSize(new Vector2Int(width, height));
            return space;
        }

        [TestCase(1f, -1.5f, -2.5f)]
        [TestCase(2f, -3f, -5f)]
        public void CellToWorld_ShouldPlaceBottomLeftCell_WhenBoardIsCentred(float cellSize, float x, float z)
        {
            BoardSpace space = Create(cellSize, 4, 6);

            Assert.That(space.CellToWorld(Vector2Int.zero), Is.EqualTo(new Vector3(x, 0f, z)));
        }

        [TestCase(1f)]
        [TestCase(2f)]
        public void WorldToCell_ShouldReturnSameCell_WhenRoundTrippingEveryCell(float cellSize)
        {
            BoardSpace space = Create(cellSize, 4, 6);

            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 6; y++)
            {
                var cell = new Vector2Int(x, y);
                Assert.That(space.WorldToCell(space.CellToWorld(cell)), Is.EqualTo(cell));
            }
        }

        [Test]
        public void WorldToCell_ShouldMapCellEdgeToHigherCell_WhenPointIsOnTheBorder()
        {
            BoardSpace space = Create(1f, 4, 6);

            Assert.That(space.WorldToCell(new Vector3(0f, 0f, 0f)), Is.EqualTo(new Vector2Int(2, 3)));
        }

        [Test]
        public void Contains_ShouldBeFalse_WhenPointIsBeyondTheBoardEdge()
        {
            BoardSpace space = Create(1f, 4, 6);

            Vector2Int outside = space.WorldToCell(new Vector3(-2.1f, 0f, 0f));

            Assert.That(outside, Is.EqualTo(new Vector2Int(-1, 3)));
            Assert.That(space.Contains(outside), Is.False);
        }

        [Test]
        public void TryProjectToBoard_ShouldReturnPointOnPlane_WhenRayPointsDownAtTheBoard()
        {
            BoardSpace space = Create(1f, 4, 6);
            var ray = new Ray(new Vector3(1f, 10f, -2f), Vector3.down);

            bool hit = space.TryProjectToBoard(ray, out Vector3 point);

            Assert.That(hit, Is.True);
            Assert.That(point, Is.EqualTo(new Vector3(1f, 0f, -2f)));
        }

        [Test]
        public void TryProjectToBoard_ShouldFail_WhenRayRunsParallelToTheBoard()
        {
            BoardSpace space = Create(1f, 4, 6);

            Assert.That(space.TryProjectToBoard(new Ray(Vector3.up, Vector3.right), out _), Is.False);
        }

        [Test]
        public void WorldSize_ShouldScaleWithCellSize_WhenSizeIsSet()
        {
            Assert.That(Create(2f, 4, 6).WorldSize, Is.EqualTo(new Vector3(8f, 0f, 12f)));
        }

        [Test]
        public void Constructor_ShouldThrow_WhenCellSizeIsNotPositive()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new BoardSpace(0f));
        }

        [Test]
        public void SetSize_ShouldThrow_WhenSizeIsNotPositive()
        {
            var space = new BoardSpace(1f);

            Assert.Throws<ArgumentOutOfRangeException>(() => space.SetSize(new Vector2Int(0, 3)));
        }
    }
}
