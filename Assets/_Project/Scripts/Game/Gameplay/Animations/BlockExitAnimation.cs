using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    /// <summary>Slides a block out through its door while it shrinks to nothing. Runs on unscaled time.</summary>
    public readonly struct BlockExitAnimation : IAnimation<BlockExitAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Transform Target;
            public readonly Vector3 LocalEnd;
            public readonly float Duration;
            public readonly Ease Ease;
            public readonly TweenCallback OnFinished;

            public Config(Transform target, Vector3 localEnd, float duration, Ease ease, TweenCallback onFinished)
            {
                Target = target;
                LocalEnd = localEnd;
                Duration = duration;
                Ease = ease;
                OnFinished = onFinished;
            }
        }

        public Sequence Play(in Config config)
        {
            return DOTween.Sequence()
                .Join(config.Target.DOLocalMove(config.LocalEnd, config.Duration).SetEase(config.Ease))
                .Join(config.Target.DOScale(0f, config.Duration).SetEase(config.Ease))
                .OnComplete(config.OnFinished)
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
