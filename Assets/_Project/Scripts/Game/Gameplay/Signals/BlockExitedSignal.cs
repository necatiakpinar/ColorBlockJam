using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// A block left the board through a door, moving in <see cref="Direction"/>, from its last cell offset on
    /// the board (<see cref="Offset"/>, lined up with the door).
    /// </summary>
    public readonly struct BlockExitedSignal
    {
        public readonly int BlockId;
        public readonly Vector2Int Direction;
        public readonly Vector2Int Offset;

        public BlockExitedSignal(int blockId, Vector2Int direction, Vector2Int offset)
        {
            BlockId = blockId;
            Direction = direction;
            Offset = offset;
        }
    }
}
