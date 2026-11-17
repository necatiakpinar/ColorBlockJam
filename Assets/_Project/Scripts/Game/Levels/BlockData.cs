using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>A block as authored: one colour and the board cells it covers at the start (DA-18).</summary>
    [Serializable]
    public sealed class BlockData
    {
        [SerializeField]
        BlockColor _color;

        [SerializeField]
        List<Vector2Int> _cells;

        // Unity calls this when it creates a block added in the inspector.
        BlockData()
        {
            _cells = new List<Vector2Int>();
        }

        public BlockData(BlockColor color, IEnumerable<Vector2Int> cells)
        {
            _color = color;
            _cells = new List<Vector2Int>(cells);
        }

        public BlockColor Color => _color;
        public IReadOnlyList<Vector2Int> Cells => _cells;
    }
}
