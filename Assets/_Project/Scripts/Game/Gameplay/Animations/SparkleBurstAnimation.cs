using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    public readonly struct SparkleBurstAnimation : IAnimation<SparkleBurstAnimation.Config>
    {
        const float GlowFromScale = 0.3f;
        const float GlowToScale = 1.4f;
        const float PeakScale = 1f;
        const float GrowShare = 0.35f;

        public readonly struct Config
        {
            public readonly Image[] Sparkles;
            public readonly Image Glow;
            public readonly float Distance;
            public readonly float Duration;

            public Config(Image[] sparkles, Image glow, float distance, float duration)
            {
                Sparkles = sparkles;
                Glow = glow;
                Distance = distance;
                Duration = duration;
            }
        }

        public Sequence Play(in Config config)
        {
            Sequence sequence = DOTween.Sequence();
            float grow = config.Duration * GrowShare;
            int count = config.Sparkles.Length;
            for (int i = 0; i < count; i++)
            {
                Image sparkle = config.Sparkles[i];
                RectTransform rect = sparkle.rectTransform;
                float angle = 360f * i / count * Mathf.Deg2Rad;
                float reach = config.Distance * (i % 2 == 0 ? 1f : 0.7f);
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.zero;
                sparkle.color = Opaque(sparkle.color);
                sequence.Insert(0f, rect.DOAnchorPos(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * reach, config.Duration).SetEase(Ease.OutCubic));
                sequence.Insert(0f, rect.DOScale(PeakScale, grow).SetEase(Ease.OutQuad));
                sequence.Insert(grow, rect.DOScale(0f, config.Duration - grow).SetEase(Ease.InQuad));
                sequence.Insert(grow, sparkle.DOFade(0f, config.Duration - grow));
            }

            RectTransform glow = config.Glow.rectTransform;
            glow.localScale = Vector3.one * GlowFromScale;
            config.Glow.color = Opaque(config.Glow.color);
            sequence.Insert(0f, glow.DOScale(GlowToScale, config.Duration).SetEase(Ease.OutCubic));
            sequence.Insert(0f, config.Glow.DOFade(0f, config.Duration).SetEase(Ease.InQuad));
            return sequence.SetUpdate(true).SetLink(config.Glow.gameObject);
        }

        static Color Opaque(Color color) => new(color.r, color.g, color.b, 1f);
    }
}
