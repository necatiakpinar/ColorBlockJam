using TMPro;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View.Blockers
{
    /// <summary>
    /// The ice blocker on screen (IceBlocker.prefab, blockers.md): the count of exits the frozen block still waits
    /// for, lying on top of it. The block's ice material is set by <see cref="BlockLayerView"/>. Pooled.
    /// </summary>
    public sealed class IceBlockerView : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _label;

        public string CountText => _label.text;

        public void Show(int exitsLeft) => _label.SetText("{0}", exitsLeft);
    }
}
