namespace ColorBlockJam.Game.Levels.Blockers
{
    /// <summary>The one axis an arrow block may move along (T-35, DA-39).</summary>
    public enum ArrowAxis
    {
        /// <summary>No arrow: the block moves freely.</summary>
        None = 0,

        /// <summary>Left and right only.</summary>
        Horizontal = 1000,

        /// <summary>Up and down only.</summary>
        Vertical = 2000,
    }
}
