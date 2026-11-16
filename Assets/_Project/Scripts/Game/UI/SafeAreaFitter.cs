using UnityEngine;

namespace RollicCase.Game.UI
{
    /// <summary>
    /// Keeps a canvas content root inside <see cref="Screen.safeArea"/>. Re-fits when the canvas
    /// resizes (resolution or simulator change) instead of polling every frame. Backgrounds stay
    /// outside this root so they still fill the whole screen.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        RectTransform _rect;

        void Awake()
        {
            _rect = (RectTransform)transform;
            Fit();
        }

        void OnRectTransformDimensionsChange()
        {
            // Unity can call this before Awake, while the hierarchy is first laid out.
            if (_rect != null)
                Fit();
        }

        void Fit()
        {
            var area = new SafeArea(Screen.safeArea, new Vector2(Screen.width, Screen.height));
            _rect.anchorMin = area.AnchorMin;
            _rect.anchorMax = area.AnchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
