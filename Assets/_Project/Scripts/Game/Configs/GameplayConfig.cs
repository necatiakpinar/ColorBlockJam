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

        [Tooltip("Seconds between winning a level and the next one starting, while there is no win popup "
            + "yet (T-21): long enough to see the last block leave (DA-23).")]
        [SerializeField, Min(0f)]
        float _nextLevelDelay = 1f;

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

        public FadeScaleInSettings HudReveal => _hudReveal;
        public float NextLevelDelay => _nextLevelDelay;
        public int ContinueSeconds => _continueSeconds;
        public int ContinueCost => _continueCost;
        public HapticImpact ExitHaptic => _exitHaptic;
    }
}
