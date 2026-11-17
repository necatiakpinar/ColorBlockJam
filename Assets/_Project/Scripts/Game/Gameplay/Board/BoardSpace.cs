using System;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>
    /// The only conversion between board cells and world space. The board lies on the XZ plane at
    /// Y = 0, centred on the world origin; cell (0, 0) is the bottom-left cell and +Y in cells is +Z
    /// in the world. Level data and rules never see world units.
    /// </summary>
    public sealed class BoardSpace
    {
        static readonly Plane BoardPlane = new(Vector3.up, Vector3.zero);

        readonly float _cellSize;
        Vector3 _origin;

        public BoardSpace(float cellSize)
        {
            if (cellSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Cell size must be positive.");

            _cellSize = cellSize;
        }

        public float CellSize => _cellSize;
        public Vector2Int Size { get; private set; }
        public Vector3 WorldSize => new(Size.x * _cellSize, 0f, Size.y * _cellSize);

        /// <summary>
        /// World position of the board's bottom-left corner. Board views parent their parts to a root placed
        /// here and scaled by <see cref="CellSize"/>, so the parts themselves are laid out in cell units.
        /// </summary>
        public Vector3 Origin => _origin;

        public void SetSize(Vector2Int size)
        {
            if (size.x <= 0 || size.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Board size must be positive.");

            Size = size;
            _origin = new Vector3(-size.x * _cellSize * 0.5f, 0f, -size.y * _cellSize * 0.5f);
        }

        /// <summary>World position of a cell's centre on the board plane.</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return _origin + new Vector3((cell.x + 0.5f) * _cellSize, 0f, (cell.y + 0.5f) * _cellSize);
        }

        /// <summary>The cell under a world point; may lie outside the board (see <see cref="Contains"/>).</summary>
        public Vector2Int WorldToCell(Vector3 world)
        {
            Vector3 local = world - _origin;
            return new Vector2Int(Mathf.FloorToInt(local.x / _cellSize), Mathf.FloorToInt(local.z / _cellSize));
        }

        /// <summary>
        /// Where a ray (e.g. from a touch) meets the board plane. False when the ray runs parallel to it
        /// or points away from it.
        /// </summary>
        public bool TryProjectToBoard(Ray ray, out Vector3 worldPoint)
        {
            bool hit = BoardPlane.Raycast(ray, out float distance);
            worldPoint = hit ? ray.GetPoint(distance) : default;
            return hit;
        }

        public bool Contains(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Size.x && cell.y < Size.y;
        }
    }
}
