using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Scenes
{
    public interface ISceneLoader
    {
        string GetSceneName(SceneId sceneId);

        UniTask LoadAsync(SceneId sceneId, CancellationToken cancellationToken);
    }
}
