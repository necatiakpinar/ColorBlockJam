using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// The rocket took one cell out of a block that still has others (DA-36); <see cref="Cell"/> is where it was on
    /// the board.
    /// </summary>
    public readonly struct BlockCellRemovedSignal
    {
        public readonly int BlockId;
        public readonly Vector2Int Cell;

        public BlockCellRemovedSignal(int blockId, Vector2Int cell)
        {
            BlockId = blockId;
            Cell = cell;
        }
    }
}
