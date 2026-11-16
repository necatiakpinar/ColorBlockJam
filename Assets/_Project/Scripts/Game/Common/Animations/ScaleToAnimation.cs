using DG.Tweening;
using RollicCase.Core.Animation;
using UnityEngine;

namespace RollicCase.Game.Common.Animations
{
    /// <summary>Scales a transform uniformly from its current scale to a target. Runs on unscaled time.</summary>
    public readonly struct ScaleToAnimation : IAnimation<ScaleToAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Transform Target;
            public readonly float EndScale;
            public readonly float Duration;
            public readonly Ease Ease;

            public Config(Transform target, float endScale, float duration, Ease ease)
            {
                Target = target;
                EndScale = endScale;
                Duration = duration;
                Ease = ease;
            }
        }

        public Sequence Play(in Config config)
        {
            return DOTween.Sequence()
                .Append(config.Target.DOScale(config.EndScale, config.Duration).SetEase(config.Ease))
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
