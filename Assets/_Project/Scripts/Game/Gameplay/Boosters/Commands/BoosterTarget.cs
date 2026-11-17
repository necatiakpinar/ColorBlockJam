using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    /// <summary>The block and cell the player tapped for a targeted booster.</summary>
    public readonly struct BoosterTarget
    {
        public readonly int BlockId;
        public readonly Vector2Int Cell;

        public BoosterTarget(int blockId, Vector2Int cell)
        {
            BlockId = blockId;
            Cell = cell;
        }

        /// <summary>No target: the booster works at once.</summary>
        public static BoosterTarget None => new(-1, default);
    }
}
