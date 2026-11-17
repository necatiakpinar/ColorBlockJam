using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Gameplay.Board;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    public sealed class VacuumBoosterCommand : IBoosterCommand
    {
        readonly ActiveBoard _board;

        public VacuumBoosterCommand(ActiveBoard board) => _board = board;

        public BoosterType Type => BoosterType.Vacuum;
        public bool NeedsTarget => true;
        public bool CanExecute => true;

        public void Execute(in BoosterTarget target) => _board.RemoveColor(_board.Current.Blocks[target.BlockId].Color);
    }
}
