using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// Builds the selection outline of a block: one closed band that hugs the block's footprint, a small gap
    /// outside it, with corners rounded like the block's own (convex corners on a wider arc, notches on a
    /// tighter one), so it traces any polyomino. Coordinates are cells, relative to <c>origin</c>.
    /// The band is built at <see cref="OutlineSettings.BaseHeight"/>; each vertex carries its outward normal
    /// and, in uv0.x, how far up the top rim is. The outline shader lifts every vertex whose side the camera
    /// cannot see up to that rim, so the band traces the block's on-screen silhouette from any angle.
    /// </summary>
    public sealed class OutlineMeshBuilder
    {
        readonly HashSet<Vector2Int> _own = new();
        readonly Dictionary<Vector2Int, List<Vector2Int>> _edgesFrom = new();
        readonly HashSet<(Vector2Int, Vector2Int)> _used = new();
        readonly List<Vector2Int> _corners = new();
        readonly List<Vector2> _path = new();

        public void Build(IReadOnlyList<Vector2Int> cells, Vector3 origin, in OutlineSettings settings,
            List<Vector3> vertices, List<Vector3> normals, List<Vector2> lifts, List<int> triangles)
        {
            vertices.Clear();
            normals.Clear();
            lifts.Clear();
            triangles.Clear();
            CollectBoundary(cells);

            foreach (KeyValuePair<Vector2Int, List<Vector2Int>> start in _edgesFrom)
            {
                foreach (Vector2Int end in start.Value)
                {
                    if (_used.Contains((start.Key, end)))
                        continue;
                    TraceCorners(start.Key, end);
                    BuildPath(settings);
                    AddBand(origin, settings, vertices, normals, lifts, triangles);
                }
            }
        }

        /// <summary>Every cell edge that borders a cell outside the block, directed with the block on its left.</summary>
        void CollectBoundary(IReadOnlyList<Vector2Int> cells)
        {
            _own.Clear();
            _edgesFrom.Clear();
            _used.Clear();
            for (int i = 0; i < cells.Count; i++)
                _own.Add(cells[i]);

            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int c = cells[i];
                if (!_own.Contains(c + Vector2Int.down))
                    AddEdge(c, c + Vector2Int.right);
                if (!_own.Contains(c + Vector2Int.right))
                    AddEdge(c + Vector2Int.right, c + Vector2Int.one);
                if (!_own.Contains(c + Vector2Int.up))
                    AddEdge(c + Vector2Int.one, c + Vector2Int.up);
                if (!_own.Contains(c + Vector2Int.left))
                    AddEdge(c + Vector2Int.up, c);
            }
        }

        void AddEdge(Vector2Int from, Vector2Int to)
        {
            if (!_edgesFrom.TryGetValue(from, out List<Vector2Int> ends))
            {
                ends = new List<Vector2Int>(1);
                _edgesFrom.Add(from, ends);
            }

            ends.Add(to);
        }

        /// <summary>
        /// Walks one boundary loop from the given edge and keeps only the points where it turns. Where two
        /// loops touch at a single point it turns left, so each loop stays around its own cells.
        /// </summary>
        void TraceCorners(Vector2Int from, Vector2Int to)
        {
            _corners.Clear();
            Vector2Int start = from;
            while (true)
            {
                _used.Add((from, to));
                Vector2Int direction = to - from;
                Vector2Int next = NextEnd(to, direction);
                if (next - to != direction)
                    _corners.Add(to);
                from = to;
                to = next;
                if (from == start && _used.Contains((from, to)))
                    break;
            }
        }

        Vector2Int NextEnd(Vector2Int at, Vector2Int incoming)
        {
            List<Vector2Int> ends = _edgesFrom[at];
            if (ends.Count == 1)
                return ends[0];
            var left = new Vector2Int(-incoming.y, incoming.x);
            for (int i = 0; i < ends.Count; i++)
                if (ends[i] - at == left)
                    return ends[i];
            return ends[0];
        }

        /// <summary>
        /// The band's centre line: the corners pushed out by the gap plus half the band, each one replaced by
        /// a quarter arc. Convex corners take the block's radius plus that offset, notches the radius minus it.
        /// </summary>
        void BuildPath(in OutlineSettings settings)
        {
            _path.Clear();
            float offset = settings.Gap + settings.Width * 0.5f;
            int count = _corners.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2 previous = _corners[(i + count - 1) % count];
                Vector2 corner = _corners[i];
                Vector2 next = _corners[(i + 1) % count];
                Vector2 a = (corner - previous).normalized;
                Vector2 b = (next - corner).normalized;
                // The block is on the left of the walk, so outward is the right-hand normal.
                Vector2 shifted = corner + (RightOf(a) + RightOf(b)) * offset;
                bool convex = a.x * b.y - a.y * b.x > 0f;
                float radius = Mathf.Max(0f, convex ? settings.CornerRadius + offset : settings.CornerRadius - offset);
                Vector2 centre = shifted - a * radius + b * radius;
                for (int s = 0; s <= settings.CornerSegments; s++)
                {
                    float t = s * (Mathf.PI * 0.5f) / settings.CornerSegments;
                    _path.Add(centre - b * (radius * Mathf.Cos(t)) + a * (radius * Mathf.Sin(t)));
                }
            }
        }

        static Vector2 RightOf(Vector2 direction) => new(direction.y, -direction.x);

        void AddBand(Vector3 origin, in OutlineSettings settings, List<Vector3> vertices, List<Vector3> normals,
            List<Vector2> lifts, List<int> triangles)
        {
            int first = vertices.Count;
            int count = _path.Count;
            float half = settings.Width * 0.5f;
            var lift = new Vector2(settings.TopHeight - settings.BaseHeight, 0f);
            for (int i = 0; i < count; i++)
            {
                Vector2 tangent = (_path[(i + 1) % count] - _path[(i + count - 1) % count]).normalized;
                Vector2 outward = RightOf(tangent);
                Vector2 inner = _path[i] - outward * half;
                Vector2 outer = _path[i] + outward * half;
                var normal = new Vector3(outward.x, 0f, outward.y);
                vertices.Add(origin + new Vector3(inner.x, settings.BaseHeight, inner.y));
                vertices.Add(origin + new Vector3(outer.x, settings.BaseHeight, outer.y));
                normals.Add(normal);
                normals.Add(normal);
                lifts.Add(lift);
                lifts.Add(lift);
            }

            // The outline shader draws both faces, so the winding does not matter.
            for (int i = 0; i < count; i++)
            {
                int a = first + i * 2;
                int b = first + (i + 1) % count * 2;
                triangles.Add(a);
                triangles.Add(a + 1);
                triangles.Add(b + 1);
                triangles.Add(a);
                triangles.Add(b + 1);
                triangles.Add(b);
            }
        }
    }
}
