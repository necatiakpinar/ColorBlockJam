using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board.Blockers
{
    /// <summary>
    /// A rule a block carries during an attempt (blockers.md): it can keep the block from being picked up or from
    /// taking a step, and it can react to other blocks leaving the board. <see cref="BoardState"/> asks every blocker of
    /// a block before it lets the block move; it never knows which kinds there are.
    /// </summary>
    public interface IBlocker
    {
        /// <summary>The player may pick the block up.</summary>
        bool AllowsGrab { get; }

        /// <summary>The block may take this one-cell step (out through a door included).</summary>
        bool AllowsStep(Vector2Int direction);

        /// <summary>Another block left the board.</summary>
        /// <returns>True when that released the block from this blocker's hold.</returns>
        bool OnOtherBlockGone();

        /// <summary>The solver lays a board out at once: puts this blocker where it is after that many exits.</summary>
        void ResetAfter(int blocksGone);

        /// <summary>The same blocker for a piece the rocket split off the block, in its current state.</summary>
        IBlocker CloneForPiece();
    }
}
