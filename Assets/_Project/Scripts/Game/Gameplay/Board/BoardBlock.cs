using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>
    /// A block during one attempt. Only <see cref="BoardState"/> moves or removes it. The rocket booster can take
    /// single cells out of it (DA-36), so its cells may change during the attempt; the level data never does.
    /// </summary>
    public sealed class BoardBlock
    {
        readonly List<Vector2Int> _startCells;

        internal BoardBlock(int id, BlockColor color, IReadOnlyList<Vector2Int> startCells)
        {
            Id = id;
            Color = color;
            _startCells = new List<Vector2Int>(startCells);
        }

        public int Id { get; }
        public BlockColor Color { get; }
        public int CellCount => _startCells.Count;

        /// <summary>The cells it still has, where the level placed them (add <see cref="Offset"/> for the board).</summary>
        public IReadOnlyList<Vector2Int> StartCells => _startCells;

        /// <summary>How far the block has moved from where the level placed it.</summary>
        public Vector2Int Offset { get; private set; }

        /// <summary>Gone from the board: out through a door, or removed by a booster.</summary>
        public bool HasExited { get; private set; }

        public Vector2Int GetCell(int index) => _startCells[index] + Offset;

        internal void Translate(Vector2Int delta) => Offset += delta;

        internal void MarkExited() => HasExited = true;

        internal void RemoveCellAt(int index) => _startCells.RemoveAt(index);

        /// <summary>Puts the block anywhere at once; only <see cref="BoardState"/> calls it, for the solver.</summary>
        internal void Reset(Vector2Int offset, bool hasExited)
        {
            Offset = offset;
            HasExited = hasExited;
        }
    }
}
