using DG.Tweening;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.UI
{
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
