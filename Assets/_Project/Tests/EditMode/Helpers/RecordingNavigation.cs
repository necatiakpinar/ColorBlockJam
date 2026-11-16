using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RollicCase.Core.Scenes;
using RollicCase.Game.Navigation;

namespace RollicCase.Tests.EditMode.Helpers
{
    public sealed class RecordingNavigation : INavigationService
    {
        public List<SceneId> Requests { get; } = new();

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
