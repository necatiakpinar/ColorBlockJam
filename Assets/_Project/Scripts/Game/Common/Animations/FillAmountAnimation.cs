using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine.UI;

namespace ColorBlockJam.Game.Common.Animations
{
    /// <summary>Moves a filled image toward a fill amount. Runs on unscaled time.</summary>
    public readonly struct FillAmountAnimation : IAnimation<FillAmountAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Image Image;
            public readonly float TargetFill;
            public readonly float Duration;
            public readonly Ease Ease;

            public Config(Image image, float targetFill, float duration, Ease ease)
            {
                Image = image;
                TargetFill = targetFill;
                Duration = duration;
                Ease = ease;
            }
        }

        public Sequence Play(in Config config)
        {
            return DOTween.Sequence()
                .Append(config.Image.DOFillAmount(config.TargetFill, config.Duration).SetEase(config.Ease))
                .SetUpdate(true)
                .SetLink(config.Image.gameObject);
        }
    }
}
