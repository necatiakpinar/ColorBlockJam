using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    public readonly struct ExitPath
    {
        public readonly int Axis;
        public readonly Vector2Int Direction;
        public readonly WallSide Side;
        public readonly int Length;
        public readonly int CrossMin;
        public readonly int CrossMax;
        public readonly int Edge;

        public ExitPath(IReadOnlyList<Vector2Int> cells, Vector2Int offset, Vector2Int direction, Vector2Int boardSize)
        {
            Direction = direction;
            Axis = direction.x != 0 ? 0 : 1;
            int cross = 1 - Axis;
            int alongMin = int.MaxValue, alongMax = int.MinValue, crossMin = int.MaxValue, crossMax = int.MinValue;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i] + offset;
                alongMin = Mathf.Min(alongMin, cell[Axis]);
                alongMax = Mathf.Max(alongMax, cell[Axis]);
                crossMin = Mathf.Min(crossMin, cell[cross]);
                crossMax = Mathf.Max(crossMax, cell[cross]);
            }

            Length = alongMax - alongMin + 1;
            CrossMin = crossMin;
            CrossMax = crossMax;
            bool outward = direction[Axis] > 0;
            Edge = outward ? boardSize[Axis] : 0;
            Side = Axis == 0 ? (outward ? WallSide.Right : WallSide.Left) : (outward ? WallSide.Top : WallSide.Bottom);
        }

        public int Width => CrossMax - CrossMin + 1;

        public Vector3 DoorCentre(float height)
        {
            Vector2 door = Vector2.zero;
            door[Axis] = Edge;
            door[1 - Axis] = (CrossMin + CrossMax + 1) * 0.5f;
            return new Vector3(door.x, height, door.y);
        }
    }
}
