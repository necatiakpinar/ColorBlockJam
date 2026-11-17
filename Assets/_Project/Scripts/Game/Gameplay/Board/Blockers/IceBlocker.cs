using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board.Blockers
{
    /// <summary>
    /// Ice (T-34, DA-37): the block is frozen, so it can be neither picked up nor moved, until enough other blocks have
    /// left the board; each exit counts it down by one. The hammer breaks it at once (<see cref="Thaw"/>).
    /// </summary>
    public sealed class IceBlocker : IBlocker
    {
        public IceBlocker(int exits)
        {
            StartExits = exits;
            ExitsLeft = exits;
        }

        /// <summary>The exits the level asked for.</summary>
        public int StartExits { get; }

        /// <summary>Exits still needed before the block thaws.</summary>
        public int ExitsLeft { get; private set; }

        public bool IsFrozen => ExitsLeft > 0;

        public bool AllowsGrab => !IsFrozen;

        public bool AllowsStep(Vector2Int direction) => !IsFrozen;

        public bool OnOtherBlockGone()
        {
            if (!IsFrozen)
                return false;
            ExitsLeft--;
            return ExitsLeft == 0;
        }

        public void ResetAfter(int blocksGone) => ExitsLeft = Mathf.Max(0, StartExits - blocksGone);

        public IBlocker CloneForPiece() => new IceBlocker(ExitsLeft);

        /// <summary>Breaks the ice at once (the hammer).</summary>
        public void Thaw() => ExitsLeft = 0;
    }
}
