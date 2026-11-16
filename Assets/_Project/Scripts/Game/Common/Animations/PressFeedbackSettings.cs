using System;
using DG.Tweening;
using UnityEngine;

namespace RollicCase.Game.Common.Animations
{
    [Serializable]
    public struct PressFeedbackSettings
    {
        [SerializeField, Range(0.5f, 1f)]
        float _pressedScale;

        [SerializeField, Min(0f)]
        float _pressDuration;

        [SerializeField]
        Ease _pressEase;

        [SerializeField, Min(0f)]
        float _releaseDuration;

        [SerializeField]
        Ease _releaseEase;

        public PressFeedbackSettings(float pressedScale, float pressDuration, Ease pressEase,
            float releaseDuration, Ease releaseEase)
        {
            _pressedScale = pressedScale;
            _pressDuration = pressDuration;
            _pressEase = pressEase;
            _releaseDuration = releaseDuration;
            _releaseEase = releaseEase;
        }

        public float PressedScale => _pressedScale;
        public float PressDuration => _pressDuration;
        public Ease PressEase => _pressEase;
        public float ReleaseDuration => _releaseDuration;
        public Ease ReleaseEase => _releaseEase;
    }
}
