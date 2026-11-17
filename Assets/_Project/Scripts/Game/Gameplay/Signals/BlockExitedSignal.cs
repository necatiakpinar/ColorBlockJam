using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
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
