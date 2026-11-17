using System.Collections.Generic;
using ColorBlockJam.Game.Gameplay.Board.Blockers;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>
    /// A block during one attempt. Only <see cref="BoardState"/> moves or removes it. The rocket booster can take
    /// single cells out of it and split it into blocks (DA-36), so its cells may change during the attempt; the
    /// level data never does. Its <see cref="Blockers"/> (blockers.md) may keep it from being picked up or moved.
    /// </summary>
    public sealed class BoardBlock
    {
        readonly List<Vector2Int> _startCells;
        readonly List<IBlocker> _blockers;

        internal BoardBlock(int id, BlockColor color, IReadOnlyList<Vector2Int> startCells, List<IBlocker> blockers)
        {
            Id = id;
            Color = color;
            _startCells = new List<Vector2Int>(startCells);
            _blockers = blockers;
        }

        public int Id { get; }
        public BlockColor Color { get; }
        public int CellCount => _startCells.Count;

        /// <summary>The cells it still has, where the level placed them (add <see cref="Offset"/> for the board).</summary>
        public IReadOnlyList<Vector2Int> StartCells => _startCells;

        /// <summary>The rules it carries (ice, ...).</summary>
        public IReadOnlyList<IBlocker> Blockers => _blockers;

        /// <summary>How far the block has moved from where the level placed it.</summary>
        public Vector2Int Offset { get; private set; }

        /// <summary>Gone from the board: out through a door, or removed by a booster.</summary>
        public bool HasExited { get; private set; }

        /// <summary>No blocker keeps the player from picking it up.</summary>
        public bool CanBeGrabbed
        {
            get
            {
                for (int i = 0; i < _blockers.Count; i++)
                    if (!_blockers[i].AllowsGrab)
                        return false;
                return true;
            }
        }

        /// <summary>The blocker of kind <typeparamref name="T"/> it carries, if any.</summary>
        public bool TryGetBlocker<T>(out T blocker) where T : class, IBlocker
        {
            for (int i = 0; i < _blockers.Count; i++)
            {
                if (_blockers[i] is not T found)
                    continue;
                blocker = found;
                return true;
            }

            blocker = null;
            return false;
        }

        public Vector2Int GetCell(int index) => _startCells[index] + Offset;

        /// <summary>Every blocker lets the block take this step.</summary>
        internal bool AllowsStep(Vector2Int direction)
        {
            for (int i = 0; i < _blockers.Count; i++)
                if (!_blockers[i].AllowsStep(direction))
                    return false;
            return true;
        }

        /// <summary>The blockers of a piece split off this block: the same rules, in their current state.</summary>
        internal List<IBlocker> CloneBlockersForPiece()
        {
            var copies = new List<IBlocker>(_blockers.Count);
            foreach (IBlocker blocker in _blockers)
                copies.Add(blocker.CloneForPiece());
            return copies;
        }

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

        /// <summary>Puts the block anywhere at once, its blockers as they are after that many exits (solver only).</summary>
        internal void Reset(Vector2Int offset, bool hasExited, int blocksGone)
        {
            Offset = offset;
            HasExited = hasExited;
            for (int i = 0; i < _blockers.Count; i++)
                _blockers[i].ResetAfter(blocksGone);
        }
    }
}
