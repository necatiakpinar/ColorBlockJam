using RollicCase.Core.Persistence;
using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay.Signals;
using VContainer.Unity;

namespace RollicCase.Game.Gameplay
{
    /// <summary>Entry point of the gameplay scene: announces the level the player is on.</summary>
    public sealed class GameplayFlow : IStartable
    {
        readonly IPlayerProgressService _playerProgress;
        readonly SignalBus _signalBus;

        public GameplayFlow(IPlayerProgressService playerProgress, SignalBus signalBus)
        {
            _playerProgress = playerProgress;
            _signalBus = signalBus;
        }

        public void Start()
        {
            _signalBus.Fire(new LevelStartedSignal(_playerProgress.CurrentLevel));
        }
    }
}
