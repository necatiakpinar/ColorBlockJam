using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Splash
{
    /// <summary>Binds the authored splash canvas. Owns the sequences it starts; builds no motion itself.</summary>
    public sealed class SplashView : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup _logoGroup;

        [SerializeField]
        RectTransform _logo;

        [SerializeField]
        Image _progressFill;

        SplashConfig _config;
        Sequence _introSequence;
        Sequence _fillSequence;

        [Inject]
        public void Construct(SplashConfig config)
        {
            _config = config;
        }

        public void PlayIntro()
        {
            _introSequence?.Kill();
            _introSequence = default(FadeScaleInAnimation).Play(
                new FadeScaleInAnimation.Config(_logoGroup, _logo, _config.LogoIntro));
        }

        public void ShowProgress(float progress)
        {
            _fillSequence?.Kill();
            _fillSequence = default(FillAmountAnimation).Play(new FillAmountAnimation.Config(
                _progressFill, progress, _config.ProgressFillSeconds, _config.ProgressFillEase));
        }

        /// <summary>Completes when the intro and the latest progress fill have finished playing.</summary>
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
