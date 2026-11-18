using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Game.Navigation;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class RecordingScreenFader : IScreenFader
    {
        readonly List<string> _log;
        UniTaskCompletionSource _fadeOut;

        public RecordingScreenFader(List<string> log = null)
        {
            _log = log ?? new List<string>();
        }

        public IReadOnlyList<string> Log => _log;

        public void HoldFadeOut() => _fadeOut = new UniTaskCompletionSource();

        public void FinishFadeOut() => _fadeOut.TrySetResult();

        public UniTask FadeOutAsync()
        {
            _log.Add("fade out");
            return _fadeOut?.Task ?? UniTask.CompletedTask;
        }

        public UniTask FadeInAsync()
        {
            _log.Add("fade in");
            return UniTask.CompletedTask;
        }
    }
}
