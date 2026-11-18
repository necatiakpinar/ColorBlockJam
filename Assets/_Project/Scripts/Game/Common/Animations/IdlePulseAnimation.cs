using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
    public readonly struct IdlePulseAnimation : IAnimation<IdlePulseAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly RectTransform Target;
            public readonly Vector2 RestPosition;
            public readonly IdlePulseSettings Settings;

            public Config(RectTransform target, Vector2 restPosition, IdlePulseSettings settings)
            {
                Target = target;
                RestPosition = restPosition;
                Settings = settings;
            }
        }

        public Sequence Play(in Config config)
        {
            IdlePulseSettings settings = config.Settings;
            config.Target.localScale = Vector3.one;
            config.Target.anchoredPosition = config.RestPosition;
            return DOTween.Sequence()
                .Append(config.Target.DOScale(settings.PeakScale, settings.HalfPeriod).SetEase(settings.Ease))
                .Join(config.Target.DOAnchorPosY(config.RestPosition.y + settings.Rise, settings.HalfPeriod).SetEase(settings.Ease))
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
