using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    /// <summary>
    /// A block sucked up by the vacuum: after its turn in the queue it rises, spins and shrinks to nothing. Runs on
    /// unscaled time.
    /// </summary>
    public readonly struct BlockVacuumAnimation : IAnimation<BlockVacuumAnimation.Config>
    {
        const float Turns = 1f;

        public readonly struct Config
        {
            public readonly Transform Target;
            public readonly float Delay;
            public readonly float Lift;
            public readonly float Duration;
            public readonly TweenCallback OnFinished;

            public Config(Transform target, float delay, float lift, float duration, TweenCallback onFinished)
            {
                Target = target;
                Delay = delay;
                Lift = lift;
                Duration = duration;
                OnFinished = onFinished;
            }
        }

        public Sequence Play(in Config config)
        {
            Transform target = config.Target;
            return DOTween.Sequence()
                .AppendInterval(config.Delay)
                .Append(target.DOLocalMoveY(target.localPosition.y + config.Lift, config.Duration).SetEase(Ease.InQuad))
                .Join(target.DOLocalRotate(new Vector3(0f, 360f * Turns, 0f), config.Duration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.InQuad))
                .Join(target.DOScale(0f, config.Duration).SetEase(Ease.InBack))
                .OnComplete(config.OnFinished)
                .SetUpdate(true)
                .SetLink(target.gameObject);
        }
    }
}
