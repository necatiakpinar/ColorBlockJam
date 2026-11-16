using System.Collections.Generic;
using NUnit.Framework;
using RollicCase.Game.Gameplay.View;
using UnityEngine;

namespace RollicCase.Tests.EditMode
{
    public sealed class OutlineMeshBuilderTests
    {
        readonly OutlineMeshBuilder _builder = new();
        readonly List<Vector3> _vertices = new();
        readonly List<int> _triangles = new();

        int StripCount(params Vector2Int[] cells)
        {
            _builder.Build(cells, Vector3.zero, 0.1f, 0.5f, _vertices, _triangles);
            Assert.That(_triangles.Count, Is.EqualTo(_vertices.Count / 4 * 6));
            return _vertices.Count / 4;
        }

        [Test]
        public void Build_ShouldTraceFourEdges_WhenTheBlockIsOneCell()
        {
            Assert.That(StripCount(Vector2Int.zero), Is.EqualTo(4));
        }

        [Test]
        public void Build_ShouldSkipInnerEdges_WhenCellsTouch()
        {
            // A 1x2 bar: 2 cells x 4 edges, minus the shared edge counted from both sides.
            Assert.That(StripCount(Vector2Int.zero, Vector2Int.right), Is.EqualTo(6));
        }

        [Test]
        public void Build_ShouldTraceTheWholePerimeter_WhenTheBlockIsAnL()
        {
            Assert.That(StripCount(Vector2Int.zero, Vector2Int.right, Vector2Int.up), Is.EqualTo(8));
        }

        [Test]
        public void Build_ShouldPlaceTheOutlineAtTheGivenHeightAroundTheCells_WhenBuilt()
        {
            _builder.Build(new[] { Vector2Int.zero }, new Vector3(-0.5f, 0f, -0.5f), 0.1f, 0.5f, _vertices, _triangles);

            foreach (Vector3 vertex in _vertices)
            {
                Assert.That(vertex.y, Is.EqualTo(0.5f));
                Assert.That(Mathf.Abs(vertex.x), Is.LessThanOrEqualTo(0.55f + 1e-5f));
                Assert.That(Mathf.Abs(vertex.z), Is.LessThanOrEqualTo(0.55f + 1e-5f));
            }
        }
    }
}
