using DG.Tweening;
using RollicCase.Game.Common.Animations;
using UnityEngine;

namespace RollicCase.Game.Configs
{
    [CreateAssetMenu(menuName = "RollicCase/Configs/Gameplay Config", fileName = "GameplayConfig")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [SerializeField]
        FadeScaleInSettings _hudReveal = new(0.35f, 0.9f, Ease.OutCubic);

        public FadeScaleInSettings HudReveal => _hudReveal;
    }
}
