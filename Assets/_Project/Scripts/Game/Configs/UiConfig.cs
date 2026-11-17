using DG.Tweening;
using ColorBlockJam.Game.Common.Animations;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/UI Config", fileName = "UiConfig")]
    public sealed class UiConfig : ScriptableObject
    {
        [SerializeField]
        PressFeedbackSettings _pressFeedback = new(0.92f, 0.08f, Ease.OutQuad, 0.18f, Ease.OutBack);

        public PressFeedbackSettings PressFeedback => _pressFeedback;
    }
}
