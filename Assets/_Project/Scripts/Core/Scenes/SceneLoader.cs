using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace RollicCase.Core.Scenes
{
    public sealed class SceneLoader : ISceneLoader
    {
        public string GetSceneName(SceneId sceneId) => sceneId switch
        {
            SceneId.Splash => "Splash",
            SceneId.Home => "Home",
            SceneId.Gameplay => "Gameplay",
            _ => throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "Scene has no build entry."),
        };

        public UniTask LoadAsync(SceneId sceneId, CancellationToken cancellationToken)
        {
            return SceneManager.LoadSceneAsync(GetSceneName(sceneId), LoadSceneMode.Single)
                .ToUniTask(cancellationToken: cancellationToken);
        }
    }
}
