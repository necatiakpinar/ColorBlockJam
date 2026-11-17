using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    /// <summary>A block whose ice broke swells for a beat and settles back to its size. Runs on unscaled time.</summary>
    public readonly struct BlockThawAnimation : IAnimation<BlockThawAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Transform Target;
            public readonly float Swell;
            public readonly float Duration;

            public Config(Transform target, float swell, float duration)
            {
                Target = target;
                Swell = swell;
                Duration = duration;
            }
        }

        public Sequence Play(in Config config)
        {
            float half = config.Duration * 0.5f;
            return DOTween.Sequence()
                .Append(config.Target.DOScale(config.Swell, half).SetEase(Ease.OutQuad))
                .Append(config.Target.DOScale(1f, half).SetEase(Ease.OutBack))
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
