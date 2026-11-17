using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Board.Blockers;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    /// <summary>Smashes the tapped block; on a frozen block it breaks the ice instead (DA-37).</summary>
    public sealed class HammerBoosterCommand : IBoosterCommand
    {
        readonly ActiveBoard _board;

        public HammerBoosterCommand(ActiveBoard board) => _board = board;

        public BoosterType Type => BoosterType.Hammer;
        public bool NeedsTarget => true;
        public bool CanExecute => true;

        public void Execute(in BoosterTarget target)
        {
            if (_board.Current.Blocks[target.BlockId].TryGetBlocker(out IceBlocker ice) && ice.IsFrozen)
                _board.Thaw(target.BlockId);
            else
                _board.RemoveBlock(target.BlockId);
        }
    }
}
