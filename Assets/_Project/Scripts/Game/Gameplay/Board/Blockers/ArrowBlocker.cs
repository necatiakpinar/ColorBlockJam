using ColorBlockJam.Game.Levels.Blockers;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board.Blockers
{
    public sealed class ArrowBlocker : IBlocker
    {
        public ArrowBlocker(ArrowAxis axis) => Axis = axis;

        public ArrowAxis Axis { get; }

        public bool AllowsGrab => true;

        public bool AllowsStep(Vector2Int direction) =>
            Axis == ArrowAxis.Horizontal ? direction.y == 0 : direction.x == 0;

        public bool OnOtherBlockGone() => false;

        public void ResetAfter(int blocksGone)
        {
        }

        public IBlocker CloneForPiece() => new ArrowBlocker(Axis);
    }
}
