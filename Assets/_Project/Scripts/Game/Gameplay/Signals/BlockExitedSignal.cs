using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>A block left the board through a door, moving in <see cref="Direction"/>.</summary>
    public readonly struct BlockExitedSignal
    {
        public readonly int BlockId;
        public readonly Vector2Int Direction;

        public BlockExitedSignal(int blockId, Vector2Int direction)
        {
            BlockId = blockId;
            Direction = direction;
        }
    }
}
