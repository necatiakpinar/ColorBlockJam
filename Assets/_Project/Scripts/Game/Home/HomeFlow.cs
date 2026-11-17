using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Navigation;
using VContainer.Unity;

namespace ColorBlockJam.Game.Home
{
    /// <summary>Entry point of the Home scene: shows the current level and routes the level button.</summary>
    public sealed class HomeFlow : IStartable, IDisposable
    {
        readonly HomeView _view;
        readonly IPlayerProgressService _playerProgress;
        readonly INavigationService _navigation;

        public HomeFlow(HomeView view, IPlayerProgressService playerProgress, INavigationService navigation)
        {
            _view = view;
            _playerProgress = playerProgress;
            _navigation = navigation;
            _view.LevelButtonClicked += OnLevelButtonClicked;
        }

        public void Start()
        {
            _view.ShowLevel(_playerProgress.CurrentLevel);
        }

        void OnLevelButtonClicked() => _navigation.GoToGameplayAsync().Forget();

        public void Dispose()
        {
            _view.LevelButtonClicked -= OnLevelButtonClicked;
        }
    }
}
