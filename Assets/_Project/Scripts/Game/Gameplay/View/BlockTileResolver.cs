using System.Collections.Generic;
using UnityEngine;

namespace RollicCase.Game.Gameplay.View
{
    /// <summary>
    /// Picks the modular pieces that draw a block of any shape. Every cell is split into four quarters, one
    /// per corner. A quarter looks at its two neighbours toward that corner: both in the block → Center, one →
    /// Edge (wall on the open side), none → OuterCorner. Where three of the four cells around a grid vertex
    /// belong to the block, one InnerCorner piece fills those three quarters with a concave curve instead.
    /// Runs once per block when a level loads.
    /// </summary>
    public sealed class BlockTileResolver
    {
        static readonly Vector2Int[] Corners = { new(1, 1), new(1, -1), new(-1, -1), new(-1, 1) };

        public List<BlockPiece> Resolve(IReadOnlyList<Vector2Int> cells)
        {
            var own = new HashSet<Vector2Int>(cells);
            var filledVertices = new HashSet<Vector2Int>();
            var pieces = new List<BlockPiece>();

            foreach (Vector2Int cell in cells)
            {
                var centre = new Vector3(cell.x + 0.5f, 0f, cell.y + 0.5f);
                foreach (Vector2Int corner in Corners)
                {
                    bool horizontal = own.Contains(cell + new Vector2Int(corner.x, 0));
                    bool vertical = own.Contains(cell + new Vector2Int(0, corner.y));
                    bool diagonal = own.Contains(cell + corner);

                    if (CountOwned(horizontal, vertical, diagonal) == 2)
                    {
                        AddInnerCorner(cell, corner, horizontal, vertical, filledVertices, pieces);
                        continue;
                    }

                    var mirror = new Vector3(corner.x, 1f, corner.y);
                    if (horizontal && vertical)
                        pieces.Add(new BlockPiece(BlockPieceKind.Center, centre, 0f, mirror));
                    else if (horizontal)
                        pieces.Add(new BlockPiece(BlockPieceKind.Edge, centre, 0f, mirror));
                    else if (vertical)
                        // Swaps the authored axes so the wall lands on the open horizontal side.
                        pieces.Add(new BlockPiece(BlockPieceKind.Edge, centre, 90f, new Vector3(-corner.y, 1f, corner.x)));
                    else
                        pieces.Add(new BlockPiece(BlockPieceKind.OuterCorner, centre, 0f, mirror));
                }
            }

            return pieces;
        }

        /// <summary>Neighbours of the quarter's cell around its vertex that belong to the block (0 to 3).</summary>
        int CountOwned(bool horizontal, bool vertical, bool diagonal) =>
            (horizontal ? 1 : 0) + (vertical ? 1 : 0) + (diagonal ? 1 : 0);

        void AddInnerCorner(Vector2Int cell, Vector2Int corner, bool horizontal, bool vertical,
            HashSet<Vector2Int> filledVertices, List<BlockPiece> pieces)
        {
            // Vertices are keyed in doubled coordinates so they stay integers.
            var vertexKey = new Vector2Int(cell.x * 2 + 1 + corner.x, cell.y * 2 + 1 + corner.y);
            if (!filledVertices.Add(vertexKey))
                return;

            // The notch points at the one cell around the vertex that is not part of the block.
            Vector2Int open = !horizontal ? new Vector2Int(corner.x, 0)
                : !vertical ? new Vector2Int(0, corner.y)
                : corner;
            Vector2Int notch = new Vector2Int(open.x == 0 ? -corner.x : corner.x, open.y == 0 ? -corner.y : corner.y);

            var vertex = new Vector3(vertexKey.x * 0.5f, 0f, vertexKey.y * 0.5f);
            pieces.Add(new BlockPiece(BlockPieceKind.InnerCorner, vertex, 0f, new Vector3(notch.x, 1f, notch.y)));
        }
    }
}
