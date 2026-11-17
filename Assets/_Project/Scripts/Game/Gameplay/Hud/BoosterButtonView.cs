using DG.Tweening;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Common.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    /// <summary>
    /// One authored booster button of the HUD bar (ui.md §5): its stock in a blue badge, or a green "+" badge when
    /// none are left (a tap then offers one for coins). It grows while its booster waits for a target, and fades
    /// while it cannot be used.
    /// </summary>
    public sealed class BoosterButtonView : MonoBehaviour
    {
        [SerializeField]
        BoosterType _type;

        [SerializeField]
        Button _button;

        [SerializeField]
        CanvasGroup _group;

        [SerializeField]
        GameObject _countBadge;

        [SerializeField]
        TMP_Text _countLabel;

        [Tooltip("Shown instead of the count when none are left.")]
        [SerializeField]
        GameObject _buyBadge;

        Sequence _scale;

        public BoosterType Type => _type;
        public Button Button => _button;
        public string CountText => _countLabel.text;
        public bool ShowsBuyBadge => _buyBadge.activeSelf;

        public void SetCount(int count)
        {
            _countBadge.SetActive(count > 0);
            _buyBadge.SetActive(count <= 0);
            _countLabel.SetText("{0}", count);
        }

        public void SetSelected(bool selected, float scale, float duration)
        {
            _scale?.Kill();
            _scale = default(ScaleToAnimation).Play(new ScaleToAnimation.Config(transform, selected ? scale : 1f,
                duration, Ease.OutBack));
        }

        public void SetFaded(bool faded, float alpha) => _group.alpha = faded ? alpha : 1f;

        void OnDestroy() => _scale?.Kill();
    }
}
