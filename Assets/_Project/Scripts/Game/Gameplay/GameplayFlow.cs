using System;
using Cysharp.Threading.Tasks;
using RollicCase.Core.Persistence;
using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay.Hud;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Game.Navigation;
using VContainer.Unity;

namespace RollicCase.Game.Gameplay
{
    /// <summary>Entry point of the gameplay scene: announces the level and routes back to Home.</summary>
    public sealed class GameplayFlow : IStartable, IDisposable
    {
        readonly IPlayerProgressService _playerProgress;
        readonly SignalBus _signalBus;
        readonly GameplayHudView _hudView;
        readonly INavigationService _navigation;

        public GameplayFlow(IPlayerProgressService playerProgress, SignalBus signalBus, GameplayHudView hudView,
            INavigationService navigation)
        {
            _playerProgress = playerProgress;
            _signalBus = signalBus;
            _hudView = hudView;
            _navigation = navigation;
            _hudView.HomeButtonClicked += OnHomeButtonClicked;
        }

        public void Start()
        {
            _signalBus.Fire(new LevelStartedSignal(_playerProgress.CurrentLevel));
        }

        void OnHomeButtonClicked() => _navigation.GoToHomeAsync().Forget();

        public void Dispose()
        {
            _hudView.HomeButtonClicked -= OnHomeButtonClicked;
        }
    }
}
