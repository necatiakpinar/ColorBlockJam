using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Game.Navigation;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class RecordingNavigation : INavigationService
    {
        public List<SceneId> Requests { get; } = new();

        public bool IsLoading => false;

        public UniTask GoToHomeAsync()
        {
            Requests.Add(SceneId.Home);
            return UniTask.CompletedTask;
        }

        public UniTask GoToGameplayAsync()
        {
            Requests.Add(SceneId.Gameplay);
            return UniTask.CompletedTask;
        }
    }
}
