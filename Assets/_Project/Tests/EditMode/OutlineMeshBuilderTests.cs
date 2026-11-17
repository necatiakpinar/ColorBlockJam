using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Gameplay.View;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class OutlineMeshBuilderTests
    {
        const int Segments = 4;
        const float Width = 0.06f;
        const float BaseHeight = 0.01f;
        const float TopHeight = 0.33f;
        const float Gap = 0.03f;
        const float Radius = 0.27f;

        static readonly OutlineSettings Settings = new(Width, BaseHeight, TopHeight, Gap, Radius, Segments);

        readonly OutlineMeshBuilder _builder = new();
        readonly List<Vector3> _vertices = new();
        readonly List<Vector3> _normals = new();
        readonly List<Vector2> _lifts = new();
        readonly List<int> _triangles = new();

        void Build(IReadOnlyList<Vector2Int> cells) =>
            _builder.Build(cells, Vector3.zero, Settings, _vertices, _normals, _lifts, _triangles);

        void Build(params Vector2Int[] cells) => Build((IReadOnlyList<Vector2Int>)cells);

        static int VerticesFor(int corners) => corners * (Segments + 1) * 2;

        static float DistanceToRoundedBar(Vector3 vertex, Vector2 size)
        {
            Vector2 half = size * 0.5f;
            var fromCentre = new Vector2(Mathf.Abs(vertex.x - half.x), Mathf.Abs(vertex.z - half.y));
            Vector2 q = fromCentre - (half - Vector2.one * Radius);
            var outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f));
            return outside.magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - Radius;
        }

        [Test]
        public void Build_ShouldTraceOneRoundedLoop_WhenTheBlockIsOneCell()
        {
            Build(Vector2Int.zero);

            Assert.That(_vertices.Count, Is.EqualTo(VerticesFor(4)));
            Assert.That(_triangles.Count, Is.EqualTo(_vertices.Count * 3), "One quad per band segment, closed.");
        }

        [Test]
        public void Build_ShouldFollowTheWholePerimeter_WhenTheBlockIsAnL()
        {
            Build(Vector2Int.zero, Vector2Int.right, Vector2Int.up);

            Assert.That(_vertices.Count, Is.EqualTo(VerticesFor(6)), "Five convex corners and one notch.");
        }

        [Test]
        public void Build_ShouldTraceTheHoleToo_WhenTheBlockIsARing()
        {
            var ring = new List<Vector2Int>();
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (x != 1 || y != 1)
                        ring.Add(new Vector2Int(x, y));

            Build(ring);

            Assert.That(_vertices.Count, Is.EqualTo(VerticesFor(4) + VerticesFor(4)), "Outer loop and hole loop.");
        }

        [Test]
        public void Build_ShouldHugTheRoundedBlockAlongItsBottomEdge_WhenTheBlockIsABar()
        {
            Build(Vector2Int.zero, Vector2Int.right, new Vector2Int(2, 0));

            foreach (Vector3 vertex in _vertices)
            {
                Assert.That(vertex.y, Is.EqualTo(BaseHeight));
                float distance = DistanceToRoundedBar(vertex, new Vector2(3f, 1f));
                Assert.That(distance, Is.GreaterThanOrEqualTo(Gap - 1e-3f), "The band never covers the block.");
                Assert.That(distance, Is.LessThanOrEqualTo(Gap + Width + 1e-3f), "The band hugs the block.");
            }
        }

        [Test]
        public void Build_ShouldRoundTheCorners_WhenTheBlockIsOneCell()
        {
            Build(Vector2Int.zero);

            float rounded = (0.5f - Radius) * Mathf.Sqrt(2f) + Radius + Gap + Width;
            var centre = new Vector3(0.5f, BaseHeight, 0.5f);
            foreach (Vector3 vertex in _vertices)
                Assert.That(Vector3.Distance(vertex, centre), Is.LessThanOrEqualTo(rounded + 1e-3f));
        }

        [Test]
        public void Build_ShouldGiveEveryVertexItsSidesOutwardNormalAndTheRimLift_WhenBuilt()
        {
            Build(Vector2Int.zero);

            Assert.That(_normals.Count, Is.EqualTo(_vertices.Count));
            Assert.That(_lifts.Count, Is.EqualTo(_vertices.Count));
            var centre = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < _vertices.Count; i++)
            {
                Assert.That(_normals[i].y, Is.EqualTo(0f));
                Assert.That(_normals[i].magnitude, Is.EqualTo(1f).Within(1e-4f));
                var fromCentre = new Vector2(_vertices[i].x, _vertices[i].z) - centre;
                Assert.That(Vector2.Dot(fromCentre, new Vector2(_normals[i].x, _normals[i].z)), Is.GreaterThan(0f),
                    "The normal points away from the block.");
                Assert.That(_lifts[i].x, Is.EqualTo(TopHeight - BaseHeight).Within(1e-5f));
            }
        }
    }
}
