using System.Collections.Generic;
using RollicCase.Game.Levels;
using UnityEngine;

namespace RollicCase.Game.Gameplay.Board
{
    /// <summary>
    /// Decides whether a block pushed against a wall fits a door there (DA-5): same colour, and the door
    /// spans every cell the block covers along that wall. Free of allocation per query.
    /// </summary>
    internal sealed class DoorResolver
    {
        readonly DoorData[] _doors;

        public DoorResolver(IReadOnlyList<DoorData> doors)
        {
            _doors = new DoorData[doors.Count];
            for (int i = 0; i < _doors.Length; i++)
                _doors[i] = doors[i];
        }

        /// <param name="direction">A unit step toward the wall: right, left, up or down.</param>
        public bool FitsDoor(BoardBlock block, Vector2Int direction)
        {
            WallSide side = WallFacing(direction);
            bool alongX = side is WallSide.Bottom or WallSide.Top;

            int min = int.MaxValue, max = int.MinValue;
            for (int i = 0; i < block.CellCount; i++)
            {
                Vector2Int cell = block.GetCell(i);
                int value = alongX ? cell.x : cell.y;
                if (value < min) min = value;
                if (value > max) max = value;
            }

            for (int i = 0; i < _doors.Length; i++)
            {
                DoorData door = _doors[i];
                if (door.Side == side && door.Color == block.Color && door.Start <= min && door.End >= max)
                    return true;
            }

            return false;
        }

        WallSide WallFacing(Vector2Int direction)
        {
            if (direction.x > 0) return WallSide.Right;
            if (direction.x < 0) return WallSide.Left;
            return direction.y > 0 ? WallSide.Top : WallSide.Bottom;
        }
    }
}
