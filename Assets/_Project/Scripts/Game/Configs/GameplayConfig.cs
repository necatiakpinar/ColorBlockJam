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

        [Tooltip("The won coins' flight to the counter after Continue (T-51).")]
        [SerializeField]
        CoinFlightSettings _coinFlight = new();

        [Header("Low time")]
        [Tooltip("From this many seconds left the timer ticks and flashes red every second (DA-42).")]
        [SerializeField, Min(1)]
        int _lowTimeSeconds = 10;

        [Tooltip("Colour the timer flashes on each low-time second before fading back.")]
        [SerializeField]
        Color _lowTimeColor = new(1f, 0.25f, 0.25f, 1f);

        [Tooltip("Scale the timer swells to on each low-time second.")]
        [SerializeField, Min(1f)]
        float _lowTimePulseScale = 1.2f;

        [Tooltip("Seconds one low-time flash takes; keep it under a second so flashes stay apart.")]
        [SerializeField, Range(0.1f, 1f)]
        float _lowTimePulseDuration = 0.6f;

        [Header("Haptics")]
        [Tooltip("Tap when a finger picks a block.")]
        [SerializeField]
        HapticImpact _grabHaptic = HapticImpact.Medium;

        [Tooltip("One tick of the trill a leaving block plays from its door until it has slid out.")]
        [SerializeField]
        HapticImpact _exitHaptic = HapticImpact.Light;

        [Tooltip("Seconds between two ticks of the exit trill.")]
        [SerializeField, Min(0.02f)]
        float _exitTickInterval = 0.06f;

        [Tooltip("Tap when the level is won.")]
        [SerializeField]
        HapticImpact _winHaptic = HapticImpact.Heavy;

        [Tooltip("Tap when the level is lost.")]
        [SerializeField]
        HapticImpact _failHaptic = HapticImpact.Heavy;

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
        public CoinFlightSettings CoinFlight => _coinFlight;
        public HapticImpact GrabHaptic => _grabHaptic;
        public HapticImpact ExitHaptic => _exitHaptic;
        public float ExitTickInterval => _exitTickInterval;
        public HapticImpact WinHaptic => _winHaptic;
        public HapticImpact FailHaptic => _failHaptic;
        public Color LowTimeColor => _lowTimeColor;
        public float LowTimePulseScale => _lowTimePulseScale;
        public float LowTimePulseDuration => _lowTimePulseDuration;

        public bool IsLowTime(int secondsLeft) => secondsLeft > 0 && secondsLeft <= _lowTimeSeconds;
    }
}
