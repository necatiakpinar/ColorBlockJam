using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Gameplay.Board.Blockers;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>
    /// The rules of one attempt, in cells. Built fresh from <see cref="LevelData"/> every time a level
    /// starts or restarts, so nothing carries over between attempts (R-25). Blocks move one cell per
    /// <see cref="TryStep"/>; a step allocates nothing.
    /// </summary>
    public sealed class BoardState
    {
        const int Empty = -1;

        readonly int[] _occupancy;
        // Grows when the rocket splits a block into pieces (DA-36); ids stay their index.
        readonly List<BoardBlock> _blocks;
        readonly DoorResolver _doorResolver;
        readonly List<int> _thawed = new();
        readonly List<int> _splitOff = new();
        readonly BlockerFactory _blockerFactory = new();

        public BoardState(LevelData level)
        {
            Width = level.Width;
            Height = level.Height;
            _occupancy = new int[Width * Height];
            Array.Fill(_occupancy, Empty);
            _doorResolver = new DoorResolver(level.Doors);

            _blocks = new List<BoardBlock>(level.Blocks.Count);
            for (int id = 0; id < level.Blocks.Count; id++)
            {
                BlockData data = level.Blocks[id];
                _blocks.Add(new BoardBlock(id, data.Color, data.Cells, _blockerFactory.Create(data.Blockers)));
                Place(_blocks[id]);
            }

            RemainingBlocks = _blocks.Count;
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<BoardBlock> Blocks => _blocks;
        public int RemainingBlocks { get; private set; }
        public bool IsCleared => RemainingBlocks == 0;

        /// <summary>Blocks the last <see cref="RemoveCell"/> split off as blocks of their own (DA-36); empty when none.</summary>
        public IReadOnlyList<int> JustSplitOff => _splitOff;

        /// <summary>
        /// Blocks the last exit or removal released from a blocker's hold (ice thawed, T-34), in id order; empty when
        /// none were.
        /// </summary>
        public IReadOnlyList<int> JustThawed => _thawed;

        public bool TryGetBlockAt(Vector2Int cell, out int blockId)
        {
            blockId = IsInside(cell) ? _occupancy[IndexOf(cell)] : Empty;
            return blockId != Empty;
        }

        /// <summary>
        /// Pushes a block one cell. It moves when every cell it enters is free. When the push would take it
        /// past a wall it exits only if it fits a door there and nothing stands between it and the wall
        /// (DA-5); otherwise it is blocked.
        /// </summary>
        /// <param name="direction">Exactly one cell right, left, up or down.</param>
        public StepResult TryStep(int blockId, Vector2Int direction)
        {
            StepResult result = CanStep(blockId, direction);
            BoardBlock block = _blocks[blockId];
            switch (result)
            {
                case StepResult.Moved:
                    Clear(block);
                    block.Translate(direction);
                    Place(block);
                    break;
                case StepResult.Exited:
                    MarkGone(block);
                    break;
            }

            return result;
        }

        /// <summary>Takes a block off the board at once (a booster, DA-36): its cells free up and it counts as gone.</summary>
        public void RemoveBlock(int blockId)
        {
            MarkGone(RequireOnBoard(blockId));
        }

        /// <summary>Breaks a block's ice at once (the hammer on a frozen block, DA-37).</summary>
        public void Thaw(int blockId)
        {
            if (RequireOnBoard(blockId).TryGetBlocker(out IceBlocker ice))
                ice.Thaw();
        }

        /// <summary>A block left: its cells free up and every blocker of the others hears of it (ice counts down, DA-37).</summary>
        void MarkGone(BoardBlock block)
        {
            Clear(block);
            block.MarkExited();
            RemainingBlocks--;
            _thawed.Clear();
            for (int id = 0; id < _blocks.Count; id++)
            {
                BoardBlock other = _blocks[id];
                if (other.HasExited)
                    continue;
                bool released = false;
                for (int b = 0; b < other.Blockers.Count; b++)
                    released |= other.Blockers[b].OnOtherBlockGone();
                if (released)
                    _thawed.Add(other.Id);
            }
        }

        /// <summary>
        /// Takes the block's cell at <paramref name="cell"/> out of it (the rocket, DA-36). When the rest falls apart,
        /// the biggest piece (the first on a tie) stays the block and every other piece becomes a block of its own,
        /// with the same colour and ice, listed in <see cref="JustSplitOff"/>.
        /// </summary>
        /// <returns>True when that was its last cell and the block is gone.</returns>
        public bool RemoveCell(int blockId, Vector2Int cell)
        {
            _splitOff.Clear();
            BoardBlock block = RequireOnBoard(blockId);
            for (int i = 0; i < block.CellCount; i++)
            {
                if (block.GetCell(i) != cell)
                    continue;
                if (block.CellCount == 1)
                {
                    RemoveBlock(blockId);
                    return true;
                }

                _occupancy[IndexOf(cell)] = Empty;
                block.RemoveCellAt(i);
                SplitIntoPieces(block);
                return false;
            }

            throw new ArgumentException($"Block {blockId} has no cell at {cell}.", nameof(cell));
        }

        /// <summary>Every edge-connected piece of the block but the biggest becomes a new block where it lies.</summary>
        void SplitIntoPieces(BoardBlock block)
        {
            List<List<Vector2Int>> pieces = Pieces(block);
            if (pieces.Count < 2)
                return;

            int keep = 0;
            for (int i = 1; i < pieces.Count; i++)
                if (pieces[i].Count > pieces[keep].Count)
                    keep = i;

            for (int i = 0; i < pieces.Count; i++)
            {
                if (i == keep)
                    continue;
                // The new block starts where the piece lies now, so its offset starts at zero.
                var piece = new BoardBlock(_blocks.Count, block.Color, pieces[i], block.CloneBlockersForPiece());
                block.RemoveBoardCells(pieces[i]);
                _blocks.Add(piece);
                foreach (Vector2Int cell in pieces[i])
                    _occupancy[IndexOf(cell)] = piece.Id;
                RemainingBlocks++;
                _splitOff.Add(piece.Id);
            }
        }

        /// <summary>The block's board cells grouped into edge-connected pieces, in the order of its cells.</summary>
        List<List<Vector2Int>> Pieces(BoardBlock block)
        {
            var cells = new HashSet<Vector2Int>();
            for (int i = 0; i < block.CellCount; i++)
                cells.Add(block.GetCell(i));

            var pieces = new List<List<Vector2Int>>();
            var seen = new HashSet<Vector2Int>();
            var frontier = new Queue<Vector2Int>();
            for (int i = 0; i < block.CellCount; i++)
            {
                Vector2Int start = block.GetCell(i);
                if (!seen.Add(start))
                    continue;
                var piece = new List<Vector2Int>();
                frontier.Enqueue(start);
                while (frontier.Count > 0)
                {
                    Vector2Int cell = frontier.Dequeue();
                    piece.Add(cell);
                    foreach (Vector2Int step in Neighbours)
                    {
                        Vector2Int next = cell + step;
                        if (cells.Contains(next) && seen.Add(next))
                            frontier.Enqueue(next);
                    }
                }

                pieces.Add(piece);
            }

            return pieces;
        }

        static readonly Vector2Int[] Neighbours = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        BoardBlock RequireOnBoard(int blockId)
        {
            BoardBlock block = _blocks[blockId];
            if (block.HasExited)
                throw new InvalidOperationException($"Block {blockId} has already left the board.");
            return block;
        }

        /// <summary>
        /// Lays every block out at once, at <paramref name="offsets"/> or gone (<paramref name="exited"/>). The
        /// level solver uses it to visit board states with these same rules; play never does.
        /// </summary>
        internal void Arrange(IReadOnlyList<Vector2Int> offsets, IReadOnlyList<bool> exited)
        {
            Array.Fill(_occupancy, Empty);
            RemainingBlocks = 0;
            // Every exit counts every frozen block down by one, so the ice follows from how many have left.
            int gone = 0;
            for (int id = 0; id < _blocks.Count; id++)
                if (exited[id])
                    gone++;
            for (int id = 0; id < _blocks.Count; id++)
            {
                _blocks[id].Reset(offsets[id], exited[id], gone);
                if (exited[id])
                    continue;
                Place(_blocks[id]);
                RemainingBlocks++;
            }
        }

        /// <summary>Moves a block straight to <paramref name="offset"/>, which must be free (solver only).</summary>
        internal void MoveBlockTo(int blockId, Vector2Int offset)
        {
            BoardBlock block = _blocks[blockId];
            Clear(block);
            block.MoveTo(offset);
            Place(block);
        }

        /// <summary>True when the cell is on the board and empty or taken by the block itself.</summary>
        public bool IsCellFree(Vector2Int cell, int blockId) => IsInside(cell) && IsFreeFor(cell, blockId);

        /// <summary>What <see cref="TryStep"/> would do, without changing the board.</summary>
        public StepResult CanStep(int blockId, Vector2Int direction)
        {
            if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 1)
                throw new ArgumentException($"A step is one cell along one axis, not {direction}.", nameof(direction));

            BoardBlock block = _blocks[blockId];
            if (block.HasExited)
                throw new InvalidOperationException($"Block {blockId} has already left the board.");
            if (!block.AllowsStep(direction))
                return StepResult.Blocked;

            bool reachesWall = false;
            for (int i = 0; i < block.CellCount; i++)
            {
                Vector2Int next = block.GetCell(i) + direction;
                if (!IsInside(next))
                    reachesWall = true;
                else if (!IsFreeFor(next, blockId))
                    return StepResult.Blocked;
            }

            if (!reachesWall)
                return StepResult.Moved;

            return _doorResolver.FitsDoor(block, direction) && HasClearPathToWall(block, direction)
                ? StepResult.Exited
                : StepResult.Blocked;
        }

        /// <summary>A block slides out in a straight line, so every cell between it and the wall must be free.</summary>
        bool HasClearPathToWall(BoardBlock block, Vector2Int direction)
        {
            for (int i = 0; i < block.CellCount; i++)
                for (Vector2Int cell = block.GetCell(i) + direction; IsInside(cell); cell += direction)
                    if (!IsFreeFor(cell, block.Id))
                        return false;
            return true;
        }

        bool IsFreeFor(Vector2Int cell, int blockId)
        {
            int occupant = _occupancy[IndexOf(cell)];
            return occupant == Empty || occupant == blockId;
        }

        void Place(BoardBlock block)
        {
            for (int i = 0; i < block.CellCount; i++)
            {
                Vector2Int cell = block.GetCell(i);
                if (!IsInside(cell) || _occupancy[IndexOf(cell)] != Empty)
                    throw new ArgumentException($"Block {block.Id} cannot be placed at {cell}; validate the level first.");
                _occupancy[IndexOf(cell)] = block.Id;
            }
        }

        void Clear(BoardBlock block)
        {
            for (int i = 0; i < block.CellCount; i++)
                _occupancy[IndexOf(block.GetCell(i))] = Empty;
        }

        bool IsInside(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;

        int IndexOf(Vector2Int cell) => cell.y * Width + cell.x;
    }
}
