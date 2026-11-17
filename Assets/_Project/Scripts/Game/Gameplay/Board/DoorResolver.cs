using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    internal sealed class DoorResolver
    {
        readonly DoorData[] _doors;

        public DoorResolver(IReadOnlyList<DoorData> doors)
        {
            _doors = new DoorData[doors.Count];
            for (int i = 0; i < _doors.Length; i++)
                _doors[i] = doors[i];
        }

        public bool FitsDoor(BoardBlock block, Vector2Int direction)
        {
            WallSide side = WallFacing(direction);
            SpanAlongWall(block, side, out int min, out int max);
            for (int i = 0; i < _doors.Length; i++)
            {
                DoorData door = _doors[i];
                if (door.Side == side && door.Color == block.Color && door.Start <= min && door.End >= max)
                    return true;
            }

            return false;
        }

        void SpanAlongWall(BoardBlock block, WallSide side, out int min, out int max)
        {
            bool alongX = side is WallSide.Bottom or WallSide.Top;
            min = int.MaxValue;
            max = int.MinValue;
            for (int i = 0; i < block.CellCount; i++)
            {
                Vector2Int cell = block.GetCell(i);
                int value = alongX ? cell.x : cell.y;
                if (value < min) min = value;
                if (value > max) max = value;
            }
        }

        WallSide WallFacing(Vector2Int direction)
        {
            if (direction.x > 0) return WallSide.Right;
            if (direction.x < 0) return WallSide.Left;
            return direction.y > 0 ? WallSide.Top : WallSide.Bottom;
        }
    }
}
