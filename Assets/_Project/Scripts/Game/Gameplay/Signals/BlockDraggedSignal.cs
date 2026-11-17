using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BlockDraggedSignal
    {
        public readonly int BlockId;
        public readonly Vector2 Offset;

        public BlockDraggedSignal(int blockId, Vector2 offset)
        {
            BlockId = blockId;
            Offset = offset;
        }
    }
}
