using System.Threading;
using Cysharp.Threading.Tasks;

namespace RollicCase.Core.Scenes
{
    public interface ISceneLoader
    {
        string GetSceneName(SceneId sceneId);

        /// <summary>Replaces every loaded scene with <paramref name="sceneId"/>.</summary>
        UniTask LoadAsync(SceneId sceneId, CancellationToken cancellationToken);
    }
}
