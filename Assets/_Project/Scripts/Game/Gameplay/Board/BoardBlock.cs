using System.Collections.Generic;
using ColorBlockJam.Game.Gameplay.Board.Blockers;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
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

        public IReadOnlyList<Vector2Int> StartCells => _startCells;

        public IReadOnlyList<IBlocker> Blockers => _blockers;

        public Vector2Int Offset { get; private set; }

        public bool HasExited { get; private set; }

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

        internal bool AllowsStep(Vector2Int direction)
        {
            for (int i = 0; i < _blockers.Count; i++)
                if (!_blockers[i].AllowsStep(direction))
                    return false;
            return true;
        }

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

        internal void RemoveBoardCells(List<Vector2Int> boardCells)
        {
            for (int i = _startCells.Count - 1; i >= 0; i--)
                if (boardCells.Contains(_startCells[i] + Offset))
                    _startCells.RemoveAt(i);
        }

        internal void Reset(Vector2Int offset, bool hasExited, int blocksGone)
        {
            Offset = offset;
            HasExited = hasExited;
            for (int i = 0; i < _blockers.Count; i++)
                _blockers[i].ResetAfter(blocksGone);
        }
    }
}
