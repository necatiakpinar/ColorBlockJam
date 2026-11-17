using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>
    /// A block during one attempt. Only <see cref="BoardState"/> moves or removes it. The rocket booster can take
    /// single cells out of it and split it into blocks (DA-36), so its cells may change during the attempt; the
    /// level data never does.
    /// </summary>
    public sealed class BoardBlock
    {
        readonly List<Vector2Int> _startCells;

        internal BoardBlock(int id, BlockColor color, IReadOnlyList<Vector2Int> startCells, int ice)
        {
            Id = id;
            Color = color;
            _startCells = new List<Vector2Int>(startCells);
            StartIce = ice;
            Ice = ice;
        }

        public int Id { get; }
        public BlockColor Color { get; }
        public int CellCount => _startCells.Count;

        /// <summary>The cells it still has, where the level placed them (add <see cref="Offset"/> for the board).</summary>
        public IReadOnlyList<Vector2Int> StartCells => _startCells;

        /// <summary>How far the block has moved from where the level placed it.</summary>
        public Vector2Int Offset { get; private set; }

        /// <summary>The ice the level gave it (T-34).</summary>
        public int StartIce { get; }

        /// <summary>Other blocks that must still leave before it thaws (DA-37).</summary>
        public int Ice { get; private set; }

        /// <summary>Frozen in place: it cannot move until its ice reaches 0.</summary>
        public bool IsFrozen => Ice > 0;

        /// <summary>Gone from the board: out through a door, or removed by a booster.</summary>
        public bool HasExited { get; private set; }

        public Vector2Int GetCell(int index) => _startCells[index] + Offset;

        internal void Translate(Vector2Int delta) => Offset += delta;

        internal void MarkExited() => HasExited = true;

        internal void RemoveCellAt(int index) => _startCells.RemoveAt(index);

        /// <summary>Drops the cells that now lie at <paramref name="boardCells"/> (a piece that split off).</summary>
        internal void RemoveBoardCells(List<Vector2Int> boardCells)
        {
            for (int i = _startCells.Count - 1; i >= 0; i--)
                if (boardCells.Contains(_startCells[i] + Offset))
                    _startCells.RemoveAt(i);
        }

        /// <summary>One other block left; true when that thawed it.</summary>
        internal bool CountDownIce()
        {
            Ice--;
            return Ice == 0;
        }

        internal void Thaw() => Ice = 0;

        /// <summary>Puts the block anywhere at once; only <see cref="BoardState"/> calls it, for the solver.</summary>
        internal void Reset(Vector2Int offset, bool hasExited, int ice)
        {
            Offset = offset;
            HasExited = hasExited;
            Ice = ice;
        }
    }
}
