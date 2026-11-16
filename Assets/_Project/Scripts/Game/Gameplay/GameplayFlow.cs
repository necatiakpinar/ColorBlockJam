using System;
using Cysharp.Threading.Tasks;
using RollicCase.Core.Persistence;
using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay.Board;
using RollicCase.Game.Gameplay.Hud;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Game.Levels;
using RollicCase.Game.Navigation;
using VContainer.Unity;

namespace RollicCase.Game.Gameplay
{
    /// <summary>Entry point of the gameplay scene: loads the current level, announces it, routes back to Home.</summary>
    public sealed class GameplayFlow : IStartable, IDisposable
    {
        readonly IPlayerProgressService _playerProgress;
        readonly SignalBus _signalBus;
        readonly GameplayHudView _hudView;
        readonly INavigationService _navigation;
        readonly LevelProvider _levelProvider;
        readonly ActiveBoard _activeBoard;

        public GameplayFlow(IPlayerProgressService playerProgress, SignalBus signalBus, GameplayHudView hudView,
            INavigationService navigation, LevelProvider levelProvider, ActiveBoard activeBoard)
        {
            _levelProvider = levelProvider;
            _activeBoard = activeBoard;
            _playerProgress = playerProgress;
            _signalBus = signalBus;
            _hudView = hudView;
            _navigation = navigation;
            _hudView.HomeButtonClicked += OnHomeButtonClicked;
        }

        public void Start()
        {
            int level = _playerProgress.CurrentLevel;
            _activeBoard.Load(_levelProvider.GetLevel(level));
            _signalBus.Fire(new LevelStartedSignal(level));
        }

        void OnHomeButtonClicked() => _navigation.GoToHomeAsync().Forget();

        public void Dispose()
        {
            _hudView.HomeButtonClicked -= OnHomeButtonClicked;
        }
    }
}
