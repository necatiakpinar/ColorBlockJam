using DG.Tweening;
using RollicCase.Game.Common.Animations;
using UnityEngine;

namespace RollicCase.Game.Configs
{
    [CreateAssetMenu(menuName = "RollicCase/Configs/UI Config", fileName = "UiConfig")]
    public sealed class UiConfig : ScriptableObject
    {
        [SerializeField]
        PressFeedbackSettings _pressFeedback = new(0.92f, 0.08f, Ease.OutQuad, 0.18f, Ease.OutBack);

        public PressFeedbackSettings PressFeedback => _pressFeedback;
    }
}
