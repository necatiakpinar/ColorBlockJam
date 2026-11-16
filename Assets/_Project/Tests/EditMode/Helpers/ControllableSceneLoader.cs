using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RollicCase.Core.Scenes;

namespace RollicCase.Tests.EditMode.Helpers
{
    /// <summary>Records loads and keeps each one pending until the test completes it.</summary>
    public sealed class ControllableSceneLoader : ISceneLoader
    {
        UniTaskCompletionSource _pending;

        public List<SceneId> Loads { get; } = new();

        public string GetSceneName(SceneId sceneId) => sceneId.ToString();

        public UniTask LoadAsync(SceneId sceneId, CancellationToken cancellationToken)
        {
            Loads.Add(sceneId);
            _pending = new UniTaskCompletionSource();
            return _pending.Task;
        }

        public void CompleteLoad() => _pending.TrySetResult();

        public void FailLoad(System.Exception exception) => _pending.TrySetException(exception);
    }
}
