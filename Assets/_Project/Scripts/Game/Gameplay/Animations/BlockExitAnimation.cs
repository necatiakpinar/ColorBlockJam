using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    /// <summary>
    /// Settles a block at the mouth of its door, then slides it through at full size; the exit shader cuts
    /// it away at the board's edge as it goes in. Runs on unscaled time.
    /// </summary>
    public readonly struct BlockExitAnimation : IAnimation<BlockExitAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Transform Target;
            public readonly Vector3 LocalDoor;
            public readonly float SnapDuration;
            public readonly Ease SnapEase;
            public readonly Vector3 LocalEnd;
            public readonly float Duration;
            public readonly Ease Ease;
            public readonly TweenCallback OnFinished;

            /// <param name="localDoor">Where the block sits lined up at the mouth of its door.</param>
            /// <param name="duration">How long the slide from the door to <paramref name="localEnd"/> takes.</param>
            public Config(Transform target, Vector3 localDoor, float snapDuration, Ease snapEase, Vector3 localEnd,
                float duration, Ease ease, TweenCallback onFinished)
            {
                Target = target;
                LocalDoor = localDoor;
                SnapDuration = snapDuration;
                SnapEase = snapEase;
                LocalEnd = localEnd;
                Duration = duration;
                Ease = ease;
                OnFinished = onFinished;
            }
        }

        public Sequence Play(in Config config)
        {
            return DOTween.Sequence()
                .Append(config.Target.DOLocalMove(config.LocalDoor, config.SnapDuration).SetEase(config.SnapEase))
                .Append(config.Target.DOLocalMove(config.LocalEnd, config.Duration).SetEase(config.Ease))
                .OnComplete(config.OnFinished)
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
