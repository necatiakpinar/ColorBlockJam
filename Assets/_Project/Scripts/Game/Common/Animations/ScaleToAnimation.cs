using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
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
