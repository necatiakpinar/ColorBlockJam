using System;
using System.Collections.Generic;
using DG.Tweening;
using ColorBlockJam.Core.Animation;
using ColorBlockJam.Game.Configs;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    public readonly struct CoinFlightAnimation : IAnimation<CoinFlightAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly IReadOnlyList<RectTransform> Coins;
            public readonly IReadOnlyList<Vector3> From;
            public readonly Vector3 To;
            public readonly float UnitScale;
            public readonly CoinFlightSettings Settings;
            public readonly Action<int> Landed;
            public readonly GameObject Link;

            public Config(IReadOnlyList<RectTransform> coins, IReadOnlyList<Vector3> from, Vector3 to, float unitScale,
                CoinFlightSettings settings, Action<int> landed, GameObject link)
            {
                Coins = coins;
                From = from;
                To = to;
                UnitScale = unitScale;
                Settings = settings;
                Landed = landed;
                Link = link;
            }
        }

        public Sequence Play(in Config config)
        {
            CoinFlightSettings settings = config.Settings;
            Sequence sequence = DOTween.Sequence();
            int count = config.From.Count;
            for (int i = 0; i < count; i++)
            {
                RectTransform coin = config.Coins[i];
                Vector3 from = config.From[i];
                float angle = (90f + 360f * i / count) * Mathf.Deg2Rad;
                Vector3 scatter = from + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * (settings.Scatter * config.UnitScale);
                float start = i * settings.Stagger;
                float fly = start + settings.PopDuration;

                coin.position = from;
                coin.localScale = Vector3.zero;
                sequence.Insert(start, coin.DOScale(1f, settings.PopDuration).SetEase(Ease.OutBack));
                sequence.Insert(start, coin.DOMove(scatter, settings.PopDuration).SetEase(Ease.OutQuad));
                sequence.Insert(fly, coin.DOMoveX(config.To.x, settings.FlyDuration).SetEase(settings.SidewaysEase));
                sequence.Insert(fly, coin.DOMoveY(config.To.y, settings.FlyDuration).SetEase(settings.UpwardEase));
                sequence.Insert(fly, coin.DOScale(settings.ArriveScale, settings.FlyDuration).SetEase(Ease.InQuad));
                int index = i;
                Action<int> landed = config.Landed;
                sequence.InsertCallback(fly + settings.FlyDuration, () => landed(index));
            }

            return sequence.SetUpdate(true).SetLink(config.Link);
        }
    }
}
