using DG.Tweening;
using ColorBlockJam.Core.Animation;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    /// <summary>
    /// The "no" a block gives when pushed into a door it cannot use: a short punch toward the door that
    /// springs back. Plays on the block's body, not its root, so it never fights the follow tween. Runs on
    /// unscaled time.
    /// </summary>
    public readonly struct BlockRejectAnimation : IAnimation<BlockRejectAnimation.Config>
    {
        public readonly struct Config
        {
            public readonly Transform Body;
            public readonly Vector3 Punch;
            public readonly float Duration;
            public readonly int Vibrato;

            /// <param name="punch">How far toward the door the body is knocked, in its local space.</param>
            public Config(Transform body, Vector3 punch, float duration, int vibrato)
            {
                Body = body;
                Punch = punch;
                Duration = duration;
                Vibrato = vibrato;
            }
        }

        public Sequence Play(in Config config)
        {
            config.Body.localPosition = Vector3.zero;
            return DOTween.Sequence()
                .Append(config.Body.DOPunchPosition(config.Punch, config.Duration, config.Vibrato))
                .SetUpdate(true)
                .SetLink(config.Body.gameObject);
        }
    }
}
