using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// A held block was pushed into a door it cannot use (another colour, or too narrow for it), moving in
    /// <see cref="Direction"/>. Fired once per push; the block stays where it is.
    /// </summary>
    public readonly struct BlockRejectedSignal
    {
        public readonly int BlockId;
        public readonly Vector2Int Direction;

        public BlockRejectedSignal(int blockId, Vector2Int direction)
        {
            BlockId = blockId;
            Direction = direction;
        }
    }
}
