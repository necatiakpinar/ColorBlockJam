using UnityEngine;

namespace RollicCase.Game.Gameplay.Signals
{
    /// <summary>The finger let go of a block; <see cref="Offset"/> is the cell it settles on.</summary>
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
