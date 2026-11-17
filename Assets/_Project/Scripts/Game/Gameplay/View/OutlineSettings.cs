namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>Shape of the selection outline, in cells (from <c>BoardViewConfig</c>).</summary>
    public readonly struct OutlineSettings
    {
        public readonly float Width;
        /// <summary>Where the band runs along a side the camera sees: the block's bottom edge.</summary>
        public readonly float BaseHeight;
        /// <summary>Where it runs along a side turned away from the camera: the rim of the top face.</summary>
        public readonly float TopHeight;
        public readonly float Gap;
        public readonly float CornerRadius;
        public readonly int CornerSegments;

        public OutlineSettings(float width, float baseHeight, float topHeight, float gap, float cornerRadius,
            int cornerSegments)
        {
            Width = width;
            BaseHeight = baseHeight;
            TopHeight = topHeight;
            Gap = gap;
            CornerRadius = cornerRadius;
            CornerSegments = cornerSegments;
        }
    }
}
