using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Session;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    public sealed class FreezeBoosterCommand : IBoosterCommand
    {
        readonly LevelTimer _timer;
        readonly BoosterConfig _config;

        public FreezeBoosterCommand(LevelTimer timer, BoosterConfig config)
        {
            _timer = timer;
            _config = config;
        }

        public BoosterType Type => BoosterType.Freeze;
        public bool NeedsTarget => false;
        public bool CanExecute => !_timer.IsFrozen;

        public void Execute(in BoosterTarget target) => _timer.FreezeFor(_config.FreezeSeconds);
    }
}
