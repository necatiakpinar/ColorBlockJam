using DG.Tweening;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Game.Common.Animations;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Gameplay Config", fileName = "GameplayConfig")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [SerializeField]
        FadeScaleInSettings _hudReveal = new(0.35f, 0.9f, Ease.OutCubic);

        [Tooltip("Seconds between winning a level and the win popup: long enough to see the last block leave.")]
        [SerializeField, Min(0f)]
        float _winPopupDelay = 1f;

        [Header("Out of time")]
        [Tooltip("Seconds a continue adds to a lost level's timer (DA-30).")]
        [SerializeField, Min(1)]
        int _continueSeconds = 20;

        [Tooltip("Coins a continue costs (DA-30).")]
        [SerializeField, Min(1)]
        int _continueCost = 50;

        [Header("Haptics")]
        [Tooltip("Tap when a block leaves through its door.")]
        [SerializeField]
        HapticImpact _exitHaptic = HapticImpact.Light;

        [Header("Boosters")]
        [Tooltip("Scale of a booster button while its booster waits for a target.")]
        [SerializeField, Min(1f)]
        float _boosterSelectedScale = 1.15f;

        [Tooltip("Alpha of the booster buttons that cannot be used right now.")]
        [SerializeField, Range(0f, 1f)]
        float _boosterFadedAlpha = 0.4f;

        [Tooltip("Alpha of the dark backdrop around the board, and of the top bar's fade, while a booster waits for a target.")]
        [SerializeField, Range(0f, 1f)]
        float _boosterDimAlpha = 0.65f;

        [Tooltip("Seconds the booster banner, the dimming and the ice frame take to fade in or out.")]
        [SerializeField, Min(0f)]
        float _boosterFadeDuration = 0.2f;

        public FadeScaleInSettings HudReveal => _hudReveal;
        public float BoosterSelectedScale => _boosterSelectedScale;
        public float BoosterFadedAlpha => _boosterFadedAlpha;
        public float BoosterDimAlpha => _boosterDimAlpha;
        public float BoosterFadeDuration => _boosterFadeDuration;
        public float WinPopupDelay => _winPopupDelay;
        public int ContinueSeconds => _continueSeconds;
        public int ContinueCost => _continueCost;
        public HapticImpact ExitHaptic => _exitHaptic;
    }
}
