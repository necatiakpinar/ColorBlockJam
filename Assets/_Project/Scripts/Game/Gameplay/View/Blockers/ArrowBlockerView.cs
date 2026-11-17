using ColorBlockJam.Game.Levels.Blockers;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View.Blockers
{
    /// <summary>
    /// The arrow blocker on screen (ArrowBlocker.prefab, blockers.md): a double-headed arrow lying on the block along
    /// the axis it may move, as long as the block is (one, two, or three cells and more). Pooled.
    /// </summary>
    public sealed class ArrowBlockerView : MonoBehaviour
    {
        [Tooltip("Arrows for a block one, two and three or more cells long along its axis; the mesh runs along local z.")]
        [SerializeField]
        GameObject[] _arrowsByLength;

        public ArrowAxis Axis { get; private set; }

        /// <param name="span">How many cells long the block is along <paramref name="axis"/>.</param>
        public void Show(ArrowAxis axis, int span)
        {
            Axis = axis;
            int shown = Mathf.Clamp(span, 1, _arrowsByLength.Length) - 1;
            for (int i = 0; i < _arrowsByLength.Length; i++)
                _arrowsByLength[i].SetActive(i == shown);
            transform.localRotation = Quaternion.Euler(0f, axis == ArrowAxis.Horizontal ? 90f : 0f, 0f);
        }
    }
}
