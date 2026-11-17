using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BlockMovedSignal
    {
        public readonly int BlockId;
        public readonly Vector2Int Offset;

        public BlockMovedSignal(int blockId, Vector2Int offset)
        {
            BlockId = blockId;
            Offset = offset;
        }
    }
}
