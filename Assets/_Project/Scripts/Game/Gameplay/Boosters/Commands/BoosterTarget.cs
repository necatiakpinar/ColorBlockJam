using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    public readonly struct BoosterTarget
    {
        public readonly int BlockId;
        public readonly Vector2Int Cell;

        public BoosterTarget(int blockId, Vector2Int cell)
        {
            BlockId = blockId;
            Cell = cell;
        }

        public static BoosterTarget None => new(-1, default);
    }
}
