using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RollicCase.Game.Gameplay.View;
using UnityEngine;

namespace RollicCase.Tests.EditMode
{
    /// <summary>Which modular pieces draw a block shape (art-pipeline.md §4).</summary>
    public sealed class BlockTileResolverTests
    {
        readonly BlockTileResolver _resolver = new();

        static Vector2Int[] Cells(params int[] xy)
        {
            var cells = new Vector2Int[xy.Length / 2];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = new Vector2Int(xy[i * 2], xy[i * 2 + 1]);
            return cells;
        }

        static Dictionary<BlockPieceKind, int> CountByKind(List<BlockPiece> pieces) =>
            pieces.GroupBy(piece => piece.Kind).ToDictionary(group => group.Key, group => group.Count());

        static void AssertCounts(List<BlockPiece> pieces, int center, int edge, int outer, int inner)
        {
            Dictionary<BlockPieceKind, int> counts = CountByKind(pieces);
            Assert.That(counts.GetValueOrDefault(BlockPieceKind.Center), Is.EqualTo(center), "Center");
            Assert.That(counts.GetValueOrDefault(BlockPieceKind.Edge), Is.EqualTo(edge), "Edge");
            Assert.That(counts.GetValueOrDefault(BlockPieceKind.OuterCorner), Is.EqualTo(outer), "OuterCorner");
            Assert.That(counts.GetValueOrDefault(BlockPieceKind.InnerCorner), Is.EqualTo(inner), "InnerCorner");
        }

        [Test]
        public void Resolve_ShouldUseFourRoundedCorners_WhenTheBlockIsOneCell()
        {
            List<BlockPiece> pieces = _resolver.Resolve(Cells(0, 0));

            AssertCounts(pieces, center: 0, edge: 0, outer: 4, inner: 0);
            Assert.That(pieces.Select(piece => (piece.Scale.x, piece.Scale.z)), Is.EquivalentTo(new[]
            {
                (1f, 1f), (1f, -1f), (-1f, -1f), (-1f, 1f),
            }));
        }

        [Test]
        public void Resolve_ShouldJoinCellsWithEdges_WhenTheBlockIsAStraightBar()
        {
            AssertCounts(_resolver.Resolve(Cells(0, 0, 1, 0, 2, 0)), center: 0, edge: 8, outer: 4, inner: 0);
        }

        [Test]
        public void Resolve_ShouldFillTheMiddleWithCaps_WhenTheBlockIsASquare()
        {
            AssertCounts(_resolver.Resolve(Cells(0, 0, 1, 0, 0, 1, 1, 1)), center: 4, edge: 8, outer: 4, inner: 0);
        }

        [Test]
        public void Resolve_ShouldPlaceOneInnerCornerAtTheConcaveVertex_WhenTheBlockIsAnL()
        {
            // (0,0), (1,0), (0,1): the vertex at (1, 1) has three block cells around it.
            List<BlockPiece> pieces = _resolver.Resolve(Cells(0, 0, 1, 0, 0, 1));

            AssertCounts(pieces, center: 0, edge: 4, outer: 5, inner: 1);
            BlockPiece inner = pieces.Single(piece => piece.Kind == BlockPieceKind.InnerCorner);
            Assert.That(inner.Position, Is.EqualTo(new Vector3(1f, 0f, 1f)));
            // The notch faces the missing cell (1, 1), up and right of the vertex.
            Assert.That((inner.Scale.x, inner.Scale.z), Is.EqualTo((1f, 1f)));
        }

        [Test]
        public void Resolve_ShouldTurnTheNotchTowardTheMissingCell_WhenTheLIsMirrored()
        {
            // (1,0), (0,1), (1,1): the missing cell (0,0) lies down and left of the vertex (1, 1).
            BlockPiece inner = _resolver.Resolve(Cells(1, 0, 0, 1, 1, 1)).Single(piece => piece.Kind == BlockPieceKind.InnerCorner);

            Assert.That((inner.Scale.x, inner.Scale.z), Is.EqualTo((-1f, -1f)));
        }

        [Test]
        public void Resolve_ShouldTurnEdgeWallsOutward_WhenTheBlockIsAVerticalBar()
        {
            // A vertical bar's side walls face left and right: those edges are the 90° variant.
            List<BlockPiece> edges = _resolver.Resolve(Cells(0, 0, 0, 1)).Where(piece => piece.Kind == BlockPieceKind.Edge).ToList();

            Assert.That(edges.Count, Is.EqualTo(4));
            Assert.That(edges.All(piece => piece.Yaw == 90f), Is.True);
        }

        [Test]
        public void Resolve_ShouldCoverEveryQuarterOnce_WhenTheShapeIsComplex()
        {
            // A T with an inner corner on each side of the stem: 4 cells × 4 quarters = 16, and each inner
            // corner piece replaces three quarters.
            List<BlockPiece> pieces = _resolver.Resolve(Cells(0, 1, 1, 1, 2, 1, 1, 0));
            int quarters = pieces.Sum(piece => piece.Kind == BlockPieceKind.InnerCorner ? 3 : 1);

            Assert.That(quarters, Is.EqualTo(16));
            Assert.That(pieces.Count(piece => piece.Kind == BlockPieceKind.InnerCorner), Is.EqualTo(2));
        }
    }
}
