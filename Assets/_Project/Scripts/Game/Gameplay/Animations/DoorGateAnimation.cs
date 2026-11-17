using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    public readonly struct DoorGateAnimation : IAnimation<DoorGateAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Transform Segment;
            public readonly float ClosedY;
            public readonly float Depth;
            public readonly float OpenDuration;
            public readonly float HoldDuration;
            public readonly float CloseDuration;

            public Config(Transform segment, float closedY, float depth, float openDuration, float holdDuration,
                float closeDuration)
            {
                Segment = segment;
                ClosedY = closedY;
                Depth = depth;
                OpenDuration = openDuration;
                HoldDuration = holdDuration;
                CloseDuration = closeDuration;
            }
        }

        public Sequence Play(in Config config)
        {
            return DOTween.Sequence()
                .Append(config.Segment.DOLocalMoveY(config.ClosedY - config.Depth, config.OpenDuration)
                    .SetEase(Ease.OutQuad))
                .AppendInterval(config.HoldDuration)
                .Append(config.Segment.DOLocalMoveY(config.ClosedY, config.CloseDuration).SetEase(Ease.OutBack))
                .SetUpdate(true)
                .SetLink(config.Segment.gameObject);
        }
    }
}
