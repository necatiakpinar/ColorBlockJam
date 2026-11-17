using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>A block during one attempt. Only <see cref="BoardState"/> moves or removes it.</summary>
    public sealed class BoardBlock
    {
        readonly Vector2Int[] _startCells;

        internal BoardBlock(int id, BlockColor color, IReadOnlyList<Vector2Int> startCells)
        {
            Id = id;
            Color = color;
            _startCells = new Vector2Int[startCells.Count];
            for (int i = 0; i < _startCells.Length; i++)
                _startCells[i] = startCells[i];
        }

        public int Id { get; }
        public BlockColor Color { get; }
        public int CellCount => _startCells.Length;

        /// <summary>How far the block has moved from where the level placed it.</summary>
        public Vector2Int Offset { get; private set; }

        public bool HasExited { get; private set; }

        public Vector2Int GetCell(int index) => _startCells[index] + Offset;

        internal void Translate(Vector2Int delta) => Offset += delta;

        internal void MarkExited() => HasExited = true;

        /// <summary>Puts the block anywhere at once; only <see cref="BoardState"/> calls it, for the solver.</summary>
        internal void Reset(Vector2Int offset, bool hasExited)
        {
            Offset = offset;
            HasExited = hasExited;
        }
    }
}
