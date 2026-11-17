using TMPro;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View.Blockers
{
    public sealed class IceBlockerView : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _label;

        public string CountText => _label.text;

        public void Show(int exitsLeft) => _label.SetText("{0}", exitsLeft);
    }
}
