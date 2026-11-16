using UnityEngine;

namespace RollicCase.Game.Gameplay.Signals
{
    /// <summary>A block moved one cell; <see cref="Offset"/> is its new offset from its start cells.</summary>
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
