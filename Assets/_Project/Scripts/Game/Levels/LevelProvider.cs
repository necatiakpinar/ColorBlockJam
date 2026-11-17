using System;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>Maps a level number to its data. After the last catalog level the catalog loops (DA-8).</summary>
    public sealed class LevelProvider
    {
        readonly LevelCatalog _catalog;

        public LevelProvider(LevelCatalog catalog)
        {
            _catalog = catalog;
        }

        public LevelData GetLevel(int levelNumber)
        {
            if (levelNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(levelNumber), levelNumber, "Level numbers start at 1.");
            if (_catalog.Levels.Count == 0)
                throw new InvalidOperationException("The level catalog is empty; add levels with the level editor.");

            return _catalog.Levels[(levelNumber - 1) % _catalog.Levels.Count].Data;
        }
    }
}
