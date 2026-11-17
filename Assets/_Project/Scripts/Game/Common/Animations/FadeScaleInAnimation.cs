using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
    public readonly struct FadeScaleInAnimation : IAnimation<FadeScaleInAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly CanvasGroup Group;
            public readonly Transform Target;
            public readonly FadeScaleInSettings Settings;

            public Config(CanvasGroup group, Transform target, FadeScaleInSettings settings)
            {
                Group = group;
                Target = target;
                Settings = settings;
            }
        }

        public Sequence Play(in Config config)
        {
            FadeScaleInSettings settings = config.Settings;
            config.Group.alpha = 0f;
            config.Target.localScale = Vector3.one * settings.FromScale;

            return DOTween.Sequence()
                .Join(config.Group.DOFade(1f, settings.Duration).SetEase(settings.Ease))
                .Join(config.Target.DOScale(1f, settings.Duration).SetEase(settings.Ease))
                .SetUpdate(true)
                .SetLink(config.Target.gameObject);
        }
    }
}
