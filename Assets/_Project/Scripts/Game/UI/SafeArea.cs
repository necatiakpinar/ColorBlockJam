using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>Converts a screen safe area in pixels to normalized anchors for a full-screen rect.</summary>
    public readonly struct SafeArea
    {
        public readonly Vector2 AnchorMin;
        public readonly Vector2 AnchorMax;

        public SafeArea(Rect safeArea, Vector2 screenSize)
        {
            AnchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
            AnchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
        }
    }
}
