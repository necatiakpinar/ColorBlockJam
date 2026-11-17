namespace ColorBlockJam.Game.Levels
{
    /// <summary>
    /// The ordered levels the game plays (level-packs.md). At runtime they come from the level pack (bundled JSON, or
    /// a remote pack stored for this launch); the editor's <see cref="LevelCatalog"/> is the other source.
    /// </summary>
    public interface ILevelSource
    {
        int LevelCount { get; }

        /// <summary>The level at a 0-based index.</summary>
        LevelData GetLevel(int index);
    }
}
