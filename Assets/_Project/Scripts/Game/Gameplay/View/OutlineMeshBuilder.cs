using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// Builds the selection outline of a block: one flat strip along every cell edge that borders a cell
    /// outside the block, so the outline traces any shape. Strips run past their ends by half their width so
    /// corners close. Coordinates are cells, relative to <c>origin</c>.
    /// </summary>
    public sealed class OutlineMeshBuilder
    {
        static readonly Vector2Int[] Sides = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        public void Build(IReadOnlyList<Vector2Int> cells, Vector3 origin, float width, float height,
            List<Vector3> vertices, List<int> triangles)
        {
            vertices.Clear();
            triangles.Clear();
            var own = new HashSet<Vector2Int>(cells);
            float half = width * 0.5f;

            foreach (Vector2Int cell in cells)
            {
                foreach (Vector2Int side in Sides)
                {
                    if (own.Contains(cell + side))
                        continue;

                    // The edge between the cell and its open neighbour, as a line from a to b.
                    Vector2 centre = cell + new Vector2(0.5f, 0.5f) + (Vector2)side * 0.5f;
                    Vector2 along = new Vector2(side.y, side.x);
                    Vector2 a = centre - along * (0.5f + half);
                    Vector2 b = centre + along * (0.5f + half);
                    Vector2 across = (Vector2)side * half;
                    AddQuad(vertices, triangles, origin, height, a - across, b - across, b + across, a + across);
                }
            }
        }

        void AddQuad(List<Vector3> vertices, List<int> triangles, Vector3 origin, float height,
            Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            int first = vertices.Count;
            vertices.Add(origin + new Vector3(p0.x, height, p0.y));
            vertices.Add(origin + new Vector3(p1.x, height, p1.y));
            vertices.Add(origin + new Vector3(p2.x, height, p2.y));
            vertices.Add(origin + new Vector3(p3.x, height, p3.y));
            // The outline shader draws both faces, so the winding does not matter.
            triangles.Add(first);
            triangles.Add(first + 1);
            triangles.Add(first + 2);
            triangles.Add(first);
            triangles.Add(first + 2);
            triangles.Add(first + 3);
        }
    }
}
