using DG.Tweening;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// The shared behaviour of every authored popup (ui.md §3): its root holds a full-screen blocker and a
    /// panel; it stays inactive until <see cref="IPopupService"/> shows it, then fades in while the panel
    /// scales up (<see cref="UiConfig.PopupReveal"/>). Hiding is instant.
    /// </summary>
    public abstract class PopupView : MonoBehaviour, IPopup
    {
        [SerializeField]
        CanvasGroup _group;

        [SerializeField]
        RectTransform _panel;

        UiConfig _uiConfig;
        Sequence _reveal;

        [Inject]
        public void ConstructPopup(UiConfig uiConfig) => _uiConfig = uiConfig;

        public void Show()
        {
            gameObject.SetActive(true);
            _reveal?.Kill();
            _reveal = default(FadeScaleInAnimation).Play(new FadeScaleInAnimation.Config(_group, _panel, _uiConfig.PopupReveal));
        }

        public void Hide()
        {
            _reveal?.Kill();
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy() => _reveal?.Kill();
    }
}
