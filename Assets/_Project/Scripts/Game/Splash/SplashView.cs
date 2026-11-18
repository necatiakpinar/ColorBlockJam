using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.Splash
{
    public sealed class SplashView : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup _logoGroup;

        [SerializeField]
        RectTransform _logo;

        [Tooltip("The loading bar's fill; it grows from the left edge by its right anchor.")]
        [SerializeField]
        RectTransform _progressFill;

        SplashConfig _config;
        Sequence _introSequence;
        Sequence _fillSequence;

        [Inject]
        public void Construct(SplashConfig config)
        {
            _config = config;
            _progressFill.anchorMax = new Vector2(0f, _progressFill.anchorMax.y);
        }

        public float ShownProgress => _progressFill.anchorMax.x;

        public void PlayIntro()
        {
            _introSequence?.Kill();
            _introSequence = default(FadeScaleInAnimation).Play(
                new FadeScaleInAnimation.Config(_logoGroup, _logo, _config.LogoIntro));
        }

        public void ShowProgress(float progress)
        {
            _fillSequence?.Kill();
            _fillSequence = default(BarFillAnimation).Play(new BarFillAnimation.Config(
                _progressFill, progress, _config.ProgressFillSeconds, _config.ProgressFillEase));
        }

        public UniTask WaitForPresentationAsync(CancellationToken cancellation)
        {
            return UniTask.WhenAll(
                _introSequence.ToUniTask(cancellationToken: cancellation),
                _fillSequence.ToUniTask(cancellationToken: cancellation));
        }

        void OnDestroy()
        {
            _introSequence?.Kill();
            _fillSequence?.Kill();
        }
    }
}
