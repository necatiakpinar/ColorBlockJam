using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board.Blockers
{
    public interface IBlocker
    {
        bool AllowsGrab { get; }

        bool AllowsStep(Vector2Int direction);

        bool OnOtherBlockGone();

        void ResetAfter(int blocksGone);

        IBlocker CloneForPiece();
    }
}
