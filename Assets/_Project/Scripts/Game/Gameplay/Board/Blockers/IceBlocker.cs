using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board.Blockers
{
    public sealed class IceBlocker : IBlocker
    {
        public IceBlocker(int exits)
        {
            StartExits = exits;
            ExitsLeft = exits;
        }

        public int StartExits { get; }

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

        public void Thaw() => ExitsLeft = 0;
    }
}
