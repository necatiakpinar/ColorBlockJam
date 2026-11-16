using System;
using System.Collections.Generic;
using RollicCase.Game.Levels;
using UnityEngine;

namespace RollicCase.Game.Gameplay.Board
{
    /// <summary>
    /// The rules of one attempt, in cells. Built fresh from <see cref="LevelData"/> every time a level
    /// starts or restarts, so nothing carries over between attempts (brief §4.3). Blocks move one cell per
    /// <see cref="TryStep"/>; a step allocates nothing.
    /// </summary>
    public sealed class BoardState
    {
        const int Empty = -1;

        readonly int[] _occupancy;
        readonly BoardBlock[] _blocks;
        readonly DoorResolver _doorResolver;

        public BoardState(LevelData level)
        {
            Width = level.Width;
            Height = level.Height;
            _occupancy = new int[Width * Height];
            Array.Fill(_occupancy, Empty);
            _doorResolver = new DoorResolver(level.Doors);

            _blocks = new BoardBlock[level.Blocks.Count];
            for (int id = 0; id < _blocks.Length; id++)
            {
                BlockData data = level.Blocks[id];
                _blocks[id] = new BoardBlock(id, data.Color, data.Cells);
                Place(_blocks[id]);
            }

            RemainingBlocks = _blocks.Length;
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<BoardBlock> Blocks => _blocks;
        public int RemainingBlocks { get; private set; }
        public bool IsCleared => RemainingBlocks == 0;

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
                    Clear(block);
                    block.MarkExited();
                    RemainingBlocks--;
                    break;
            }

            return result;
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
