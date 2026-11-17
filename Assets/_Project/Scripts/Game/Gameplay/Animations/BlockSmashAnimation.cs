using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    public readonly struct BlockSmashAnimation : IAnimation<BlockSmashAnimation.Config>
    {
        const float SwellShare = 0.35f;

        public readonly struct Config
        {
            public readonly Transform Target;
            public readonly float Swell;
            public readonly float Duration;
            public readonly TweenCallback OnFinished;

            public Config(Transform target, float swell, float duration, TweenCallback onFinished)
            {
                Target = target;
                Swell = swell;
                Duration = duration;
                OnFinished = onFinished;
            }
        }

        public Sequence Play(in Config config)
        {
            return DOTween.Sequence()
                .Append(config.Target.DOScale(config.Swell, config.Duration * SwellShare).SetEase(Ease.OutQuad))
                .Append(config.Target.DOScale(0f, config.Duration * (1f - SwellShare)).SetEase(Ease.InBack))
                .OnComplete(config.OnFinished)
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
