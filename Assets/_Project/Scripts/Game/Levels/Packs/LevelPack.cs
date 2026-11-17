using System.Collections.Generic;

namespace ColorBlockJam.Game.Levels.Packs
{
    /// <summary>
    /// A complete, ordered set of levels as shipped or served remotely (level-packs.md). <see cref="Revision"/>
    /// grows with every content change, so a stored remote pack older than the one in the build is ignored.
    /// </summary>
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
