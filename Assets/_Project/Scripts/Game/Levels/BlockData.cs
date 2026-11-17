using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Levels.Blockers;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    [Serializable]
    public sealed class BlockData
    {
        [SerializeField]
        BlockColor _color;

        [SerializeField]
        List<Vector2Int> _cells;

        [Tooltip("Ice, and the other blocker kinds, on this block (blockers.md).")]
        [SerializeField]
        BlockerSet _blockers = new();

        BlockData()
        {
            _cells = new List<Vector2Int>();
        }

        public BlockData(BlockColor color, IEnumerable<Vector2Int> cells, BlockerSet blockers = null)
        {
            _color = color;
            _cells = new List<Vector2Int>(cells);
            _blockers = blockers ?? new BlockerSet();
        }

        public BlockColor Color => _color;
        public IReadOnlyList<Vector2Int> Cells => _cells;
        public BlockerSet Blockers => _blockers;
    }
}
