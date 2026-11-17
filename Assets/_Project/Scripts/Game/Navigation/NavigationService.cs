using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Scenes;

namespace ColorBlockJam.Game.Navigation
{
    public sealed class NavigationService : INavigationService
    {
        readonly ISceneLoader _sceneLoader;
        bool _isLoading;

        public NavigationService(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public bool IsLoading => _isLoading;

        public UniTask GoToHomeAsync() => LoadAsync(SceneId.Home);

        public UniTask GoToGameplayAsync() => LoadAsync(SceneId.Gameplay);

        async UniTask LoadAsync(SceneId sceneId)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            try
            {
                await _sceneLoader.LoadAsync(sceneId, CancellationToken.None);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
