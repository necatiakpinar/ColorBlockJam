using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    public readonly struct BoosterDimAnimation : IAnimation<BoosterDimAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly CanvasGroup Banner;
            public readonly Material Backdrop;
            public readonly CanvasGroup[] Faded;
            public readonly bool Shown;
            public readonly float DimAlpha;
            public readonly float Duration;
            public readonly GameObject Link;

            public Config(CanvasGroup banner, Material backdrop, CanvasGroup[] faded, bool shown, float dimAlpha,
                float duration, GameObject link)
            {
                Banner = banner;
                Backdrop = backdrop;
                Faded = faded;
                Shown = shown;
                DimAlpha = dimAlpha;
                Duration = duration;
                Link = link;
            }
        }

        public Sequence Play(in Config config)
        {
            Sequence sequence = DOTween.Sequence()
                .Join(config.Banner.DOFade(config.Shown ? 1f : 0f, config.Duration))
                .Join(config.Backdrop.DOFade(config.Shown ? config.DimAlpha : 0f, config.Duration));
            float partAlpha = config.Shown ? 1f - config.DimAlpha : 1f;
            foreach (CanvasGroup part in config.Faded)
                sequence.Join(part.DOFade(partAlpha, config.Duration));
            return sequence.SetEase(Ease.OutQuad).SetUpdate(true).SetLink(config.Link);
        }
    }
}
