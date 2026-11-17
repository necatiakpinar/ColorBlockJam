using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
    public readonly struct PunchScaleAnimation : IAnimation<PunchScaleAnimation.Config>
    {
        const int Vibrato = 1;
        const float Elasticity = 0.5f;

        public readonly struct Config
        {
            public readonly Transform Target;
            public readonly float Strength;
            public readonly float Duration;

            public Config(Transform target, float strength, float duration)
            {
                Target = target;
                Strength = strength;
                Duration = duration;
            }
        }

        public Sequence Play(in Config config)
        {
            config.Target.localScale = Vector3.one;
            return DOTween.Sequence()
                .Append(config.Target.DOPunchScale(Vector3.one * config.Strength, config.Duration, Vibrato, Elasticity))
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
