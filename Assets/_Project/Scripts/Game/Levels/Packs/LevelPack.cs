using System.Collections.Generic;

namespace ColorBlockJam.Game.Levels.Packs
{
    public sealed class LevelPack
    {
        public LevelPack(int revision, IReadOnlyList<LevelData> levels)
        {
            Revision = revision;
            Levels = levels;
        }

        public int Revision { get; }
        public IReadOnlyList<LevelData> Levels { get; }
    }
}
