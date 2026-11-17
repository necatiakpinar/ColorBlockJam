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

        public UniTask GoToHomeAsync() => LoadAsync(SceneId.Home);

        public UniTask GoToGameplayAsync() => LoadAsync(SceneId.Gameplay);

        async UniTask LoadAsync(SceneId sceneId)
        {
            // A second tap while a scene is loading must not queue a second load.
            if (_isLoading)
                return;

            _isLoading = true;
            try
            {
                // The load destroys the calling scene's scope and cancels its token; listening to that
                // token would report the load's own success as a cancellation.
                await _sceneLoader.LoadAsync(sceneId, CancellationToken.None);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
