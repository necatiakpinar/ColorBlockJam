using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
    /// <summary>
    /// Moves a bar's fill to a share of its track by its right anchor, so a nine-sliced fill keeps its rounded ends at
    /// any length (a Filled image would cut them off). Runs on unscaled time.
    /// </summary>
    public readonly struct BarFillAnimation : IAnimation<BarFillAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly RectTransform Fill;
            public readonly float TargetShare;
            public readonly float Duration;
            public readonly Ease Ease;

            public Config(RectTransform fill, float targetShare, float duration, Ease ease)
            {
                Fill = fill;
                TargetShare = targetShare;
                Duration = duration;
                Ease = ease;
            }
        }

        public Sequence Play(in Config config)
        {
            RectTransform fill = config.Fill;
            return DOTween.Sequence()
                .Append(DOTween.To(() => fill.anchorMax.x, x => fill.anchorMax = new Vector2(x, fill.anchorMax.y),
                    config.TargetShare, config.Duration).SetEase(config.Ease))
                .SetUpdate(true)
                .SetLink(fill.gameObject);
        }
    }
}
