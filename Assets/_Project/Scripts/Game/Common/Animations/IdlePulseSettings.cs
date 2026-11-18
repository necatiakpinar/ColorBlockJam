using System;
using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.Common.Animations
{
    [Serializable]
    public struct IdlePulseSettings
    {
        [Tooltip("Scale at the top of each breath.")]
        [SerializeField, Min(1f)]
        float _peakScale;

        [Tooltip("How far up the target floats at the top of each breath, in canvas units.")]
        [SerializeField]
        float _rise;

        [Tooltip("Seconds from rest to the top of a breath; it comes back down in the same time.")]
        [SerializeField, Min(0.05f)]
        float _halfPeriod;

        [SerializeField]
        Ease _ease;

        public IdlePulseSettings(float peakScale, float rise, float halfPeriod, Ease ease)
        {
            _peakScale = peakScale;
            _rise = rise;
            _halfPeriod = halfPeriod;
            _ease = ease;
        }

        public float PeakScale => _peakScale;
        public float Rise => _rise;
        public float HalfPeriod => _halfPeriod;
        public Ease Ease => _ease;
    }
}
