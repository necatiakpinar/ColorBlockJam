using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
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
