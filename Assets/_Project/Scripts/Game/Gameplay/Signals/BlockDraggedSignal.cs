using UnityEngine;

namespace RollicCase.Game.Gameplay.Signals
{
    /// <summary>
    /// Where a held block is drawn this frame: its offset from its start cells in fractional cells. Fired
    /// for every drag event; the board itself only changes in whole cells.
    /// </summary>
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
