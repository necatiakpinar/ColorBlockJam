using DG.Tweening;
using ColorBlockJam.Game.Common.Animations;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Splash Config", fileName = "SplashConfig")]
    public sealed class SplashConfig : ScriptableObject
    {
        [Tooltip("The splash stays up at least this long, even when booting is faster.")]
        [SerializeField, Min(0f)]
        float _minimumDisplaySeconds = 1.5f;

        [SerializeField]
        FadeScaleInSettings _logoIntro = new(0.6f, 0.85f, Ease.OutBack);

        [SerializeField, Min(0.01f)]
        float _progressFillSeconds = 0.35f;

        [SerializeField]
        Ease _progressFillEase = Ease.OutQuad;

        public float MinimumDisplaySeconds => _minimumDisplaySeconds;
        public FadeScaleInSettings LogoIntro => _logoIntro;
        public float ProgressFillSeconds => _progressFillSeconds;
        public Ease ProgressFillEase => _progressFillEase;
    }
}
