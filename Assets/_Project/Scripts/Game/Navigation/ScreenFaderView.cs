using Cysharp.Threading.Tasks;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.Navigation
{
    public sealed class ScreenFaderView : MonoBehaviour, IScreenFader
    {
        [Tooltip("The black full-screen layer above every scene; it also blocks taps while it shows.")]
        [SerializeField]
        CanvasGroup _blackout;

        UiConfig _config;
        Sequence _fade;

        [Inject]
        public void Construct(UiConfig config)
        {
            _config = config;
            _blackout.alpha = 0f;
            _blackout.blocksRaycasts = false;
        }

        public bool IsShowing => _blackout.alpha > 0f;

        public UniTask FadeOutAsync()
        {
            _blackout.blocksRaycasts = true;
            return FadeAsync(1f, _config.SceneFadeOut);
        }

        public async UniTask FadeInAsync()
        {
            await FadeAsync(0f, _config.SceneFadeIn);
            _blackout.blocksRaycasts = false;
        }

        UniTask FadeAsync(float alpha, float duration)
        {
            _fade?.Kill();
            _fade = default(CanvasFadeAnimation).Play(new CanvasFadeAnimation.Config(_blackout, alpha, duration));
            return _fade.ToUniTask(cancellationToken: destroyCancellationToken);
        }

        void OnDestroy() => _fade?.Kill();
    }
}
