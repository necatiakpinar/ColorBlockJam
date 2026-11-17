using System.Collections.Generic;
using ColorBlockJam.Game.Levels.Blockers;

namespace ColorBlockJam.Game.Gameplay.Board.Blockers
{
    public sealed class BlockerFactory
    {
        public List<IBlocker> Create(BlockerSet set)
        {
            var blockers = new List<IBlocker>();
            if (set.Ice.IsActive)
                blockers.Add(new IceBlocker(set.Ice.Exits));
            if (set.Arrow.IsActive)
                blockers.Add(new ArrowBlocker(set.Arrow.Axis));
            return blockers;
        }
    }
}
