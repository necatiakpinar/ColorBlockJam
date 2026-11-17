using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BlockDroppedSignal
    {
        public readonly int BlockId;
        public readonly Vector2Int Offset;

        public BlockDroppedSignal(int blockId, Vector2Int offset)
        {
            BlockId = blockId;
            Offset = offset;
        }
    }
}
