using ColorBlockJam.Game.Boosters;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    public interface IBoosterCommand
    {
        BoosterType Type { get; }

        bool NeedsTarget { get; }

        bool CanExecute { get; }

        void Execute(in BoosterTarget target);
    }
}
