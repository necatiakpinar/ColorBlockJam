using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
    /// <summary>Fades a canvas group from its current alpha to a target. Runs on unscaled time.</summary>
    public readonly struct CanvasFadeAnimation : IAnimation<CanvasFadeAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly CanvasGroup Group;
            public readonly float Alpha;
            public readonly float Duration;

            public Config(CanvasGroup group, float alpha, float duration)
            {
                Group = group;
                Alpha = alpha;
                Duration = duration;
            }
        }

        public Sequence Play(in Config config)
        {
            return DOTween.Sequence()
                .Append(config.Group.DOFade(config.Alpha, config.Duration).SetEase(Ease.OutQuad))
                .SetUpdate(true)
                .SetLink(config.Group.gameObject);
        }
    }
}
