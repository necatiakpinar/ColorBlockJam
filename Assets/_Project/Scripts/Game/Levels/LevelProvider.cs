using System;

namespace ColorBlockJam.Game.Levels
{
    public sealed class LevelProvider
    {
        readonly ILevelSource _levels;

        public LevelProvider(ILevelSource levels)
        {
            _levels = levels;
        }

        public LevelData GetLevel(int levelNumber)
        {
            if (levelNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(levelNumber), levelNumber, "Level numbers start at 1.");
            if (_levels.LevelCount == 0)
                throw new InvalidOperationException("There are no levels; add some with the level editor.");

            return _levels.GetLevel((levelNumber - 1) % _levels.LevelCount);
        }
    }
}
