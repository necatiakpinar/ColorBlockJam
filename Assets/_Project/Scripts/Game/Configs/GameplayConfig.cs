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

        [Header("Haptics")]
        [Tooltip("Tap when a block leaves through its door.")]
        [SerializeField]
        HapticImpact _exitHaptic = HapticImpact.Light;

        public FadeScaleInSettings HudReveal => _hudReveal;
        public HapticImpact ExitHaptic => _exitHaptic;
    }
}
