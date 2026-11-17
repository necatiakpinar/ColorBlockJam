using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Gameplay.Board;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    /// <summary>Blasts the tapped cell out of its block.</summary>
    public sealed class RocketBoosterCommand : IBoosterCommand
    {
        readonly ActiveBoard _board;

        public RocketBoosterCommand(ActiveBoard board) => _board = board;

        public BoosterType Type => BoosterType.Rocket;
        public bool NeedsTarget => true;
        public bool CanExecute => true;

        public void Execute(in BoosterTarget target) => _board.RemoveCell(target.BlockId, target.Cell);
    }
}
