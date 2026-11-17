namespace ColorBlockJam.Game.Gameplay.View
{
    public readonly struct OutlineSettings
    {
        public readonly float Width;
        public readonly float BaseHeight;
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
