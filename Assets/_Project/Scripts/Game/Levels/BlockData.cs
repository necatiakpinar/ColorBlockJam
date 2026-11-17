using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>
    /// A block as authored: one colour, the board cells it covers at the start (DA-18), and its ice: how many
    /// other blocks must leave before it thaws and can move (T-34, DA-37; 0 = not frozen).
    /// </summary>
    [Serializable]
    public sealed class BlockData
    {
        [SerializeField]
        BlockColor _color;

        [SerializeField]
        List<Vector2Int> _cells;

        [Tooltip("Other blocks that must leave the board before this one thaws; 0 = not frozen.")]
        [SerializeField, Min(0)]
        int _ice;

        // Unity calls this when it creates a block added in the inspector.
        BlockData()
        {
            _cells = new List<Vector2Int>();
        }

        public BlockData(BlockColor color, IEnumerable<Vector2Int> cells, int ice = 0)
        {
            _color = color;
            _cells = new List<Vector2Int>(cells);
            _ice = ice;
        }

        public BlockColor Color => _color;
        public IReadOnlyList<Vector2Int> Cells => _cells;
        public int Ice => _ice;
    }
}
