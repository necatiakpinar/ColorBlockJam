using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    public readonly struct TimerWarningAnimation : IAnimation<TimerWarningAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Graphic Label;
            public readonly Color WarningColor;
            public readonly Color RestColor;
            public readonly float Scale;
            public readonly float Duration;

            public Config(Graphic label, Color warningColor, Color restColor, float scale, float duration)
            {
                Label = label;
                WarningColor = warningColor;
                RestColor = restColor;
                Scale = scale;
                Duration = duration;
            }
        }

        public Sequence Play(in Config config)
        {
            Transform target = config.Label.transform;
            target.localScale = Vector3.one;
            config.Label.color = config.WarningColor;
            float half = config.Duration * 0.5f;
            return DOTween.Sequence()
                .Append(target.DOScale(config.Scale, half).SetEase(Ease.OutQuad))
                .Append(target.DOScale(1f, half).SetEase(Ease.InQuad))
                .Insert(half, config.Label.DOColor(config.RestColor, half).SetEase(Ease.InQuad))
                .SetUpdate(true)
                .SetLink(config.Label.gameObject);
        }
    }
}
