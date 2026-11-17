using UnityEngine;

namespace ColorBlockJam.Game.UI
{
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
