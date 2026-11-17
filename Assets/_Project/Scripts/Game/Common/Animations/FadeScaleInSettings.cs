using System;
using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
    [Serializable]
    public struct FadeScaleInSettings
    {
        [SerializeField, Min(0f)]
        float _duration;

        [SerializeField, Min(0f)]
        float _fromScale;

        [SerializeField]
        Ease _ease;

        public FadeScaleInSettings(float duration, float fromScale, Ease ease)
        {
            _duration = duration;
            _fromScale = fromScale;
            _ease = ease;
        }

        public float Duration => _duration;
        public float FromScale => _fromScale;
        public Ease Ease => _ease;
    }
}
