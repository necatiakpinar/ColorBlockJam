using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Gameplay.Board;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Solving
{
    /// <summary>
    /// The solver's view of a level (level-editor.md §3). Every block's legal positions, its one-cell moves, the
    /// spots it can leave from and the cells its way out needs free are worked out once, as 128-bit cell masks,
    /// by asking a real <see cref="BoardState"/>. The search then runs on bit operations alone while the rules
    /// (walls, doors, arrows, ice) still live only in <see cref="BoardState"/> and the blockers.
    /// </summary>
    internal sealed class SearchBoard
    {
        /// <summary>Right, left, up, down: the order every search step tries them in.</summary>
        public static readonly Vector2Int[] Directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        public const int MaxCells = 128;
        const int NoMove = -1;

        readonly int _width;
        readonly Vector2Int[][] _offsets;
        readonly ulong[][] _low;
        readonly ulong[][] _high;
        // Indexed [block][position * 4 + direction].
        readonly int[][] _moves;
        readonly bool[][] _exits;
        readonly ulong[][] _wayLow;
        readonly ulong[][] _wayHigh;
        // Indexed [block][blocks gone * 4 + direction]: what its blockers (arrow, ice) allow.
        readonly bool[][] _allows;
        readonly int[] _start;

        public SearchBoard(LevelData level)
        {
            if (level.Width * level.Height > MaxCells)
                throw new ArgumentException($"The solver handles boards up to {MaxCells} cells, not {level.Width}x{level.Height}.");
            _width = level.Width;
            int count = level.Blocks.Count;
            BlockCount = count;
            _offsets = new Vector2Int[count][];
            _low = new ulong[count][];
            _high = new ulong[count][];
            _moves = new int[count][];
            _exits = new bool[count][];
            _wayLow = new ulong[count][];
            _wayHigh = new ulong[count][];
            _allows = new bool[count][];
            _start = new int[count];
            for (int id = 0; id < count; id++)
                MapBlock(level, id);
            MapBlockers(level);
        }

        public int BlockCount { get; }

        public int PositionCount(int id) => _offsets[id].Length;

        /// <summary>The position the level places the block at.</summary>
        public int StartPosition(int id) => _start[id];

        public Vector2Int OffsetOf(int id, int position) => _offsets[id][position];

        public ulong Low(int id, int position) => _low[id][position];

        public ulong High(int id, int position) => _high[id][position];

        /// <summary>Whether the block's blockers let it step that way once <paramref name="gone"/> blocks have left.</summary>
        public bool Allows(int id, int gone, int direction) => _allows[id][gone * Directions.Length + direction];

        /// <summary>
        /// Where one step takes the block when the cells are free of others, or -1 when a wall stops it. The
        /// caller still checks the target's cells against the other blocks and asks <see cref="Allows"/>.
        /// </summary>
        public int MoveTo(int id, int position, int direction) => _moves[id][position * Directions.Length + direction];

        /// <summary>Whether the block leaves through a door from here, with the others at these cells.</summary>
        public bool CanLeave(int id, int position, int direction, ulong othersLow, ulong othersHigh)
        {
            int at = position * Directions.Length + direction;
            return _exits[id][at] && (_wayLow[id][at] & othersLow) == 0 && (_wayHigh[id][at] & othersHigh) == 0;
        }

        /// <summary>Positions, masks and moves, and the exits by door: the block alone on a board with every door.</summary>
        void MapBlock(LevelData level, int id)
        {
            BlockData data = level.Blocks[id];
            var offsets = new List<Vector2Int>();
            for (int y = -level.Height; y <= level.Height; y++)
                for (int x = -level.Width; x <= level.Width; x++)
                    if (FitsAt(level, data, new Vector2Int(x, y)))
                        offsets.Add(new Vector2Int(x, y));
            int positions = offsets.Count;
            _offsets[id] = offsets.ToArray();
            _start[id] = offsets.IndexOf(Vector2Int.zero);
            _low[id] = new ulong[positions];
            _high[id] = new ulong[positions];
            for (int p = 0; p < positions; p++)
                foreach (Vector2Int cell in data.Cells)
                    Set(ref _low[id][p], ref _high[id][p], cell + offsets[p]);

            // A plain copy of the block (its blockers are mapped apart, by blocks gone) and a one-cell probe that
            // finds the cells a way out needs free.
            Vector2Int probeCell = FreeCellOutside(level, data);
            bool hasProbe = probeCell.x >= 0;
            var blocks = new List<BlockData> { new(data.Color, data.Cells) };
            if (hasProbe)
                blocks.Add(new BlockData(data.Color, new[] { probeCell }));
            var board = new BoardState(new LevelData(level.Width, level.Height, level.TimerSeconds, blocks, level.Doors, level.Difficulty));
            var at = new Vector2Int[blocks.Count];
            var gone = new bool[blocks.Count];

            int steps = positions * Directions.Length;
            _moves[id] = new int[steps];
            _exits[id] = new bool[steps];
            _wayLow[id] = new ulong[steps];
            _wayHigh[id] = new ulong[steps];
            for (int p = 0; p < positions; p++)
            {
                at[0] = offsets[p];
                if (hasProbe)
                    gone[1] = true;
                board.Arrange(at, gone);
                for (int d = 0; d < Directions.Length; d++)
                {
                    int step = p * Directions.Length + d;
                    StepResult result = board.CanStep(0, Directions[d]);
                    _moves[id][step] = result == StepResult.Moved ? offsets.IndexOf(offsets[p] + Directions[d]) : NoMove;
                    _exits[id][step] = result == StepResult.Exited;
                }

                if (hasProbe)
                    MapWaysOut(level, board, id, p, at, gone, probeCell);
            }
        }

        /// <summary>For every exit from position <paramref name="p"/>: the cells a block standing there would block.</summary>
        void MapWaysOut(LevelData level, BoardState board, int id, int p, Vector2Int[] at, bool[] gone, Vector2Int probeCell)
        {
            gone[1] = false;
            for (int d = 0; d < Directions.Length; d++)
            {
                int step = p * Directions.Length + d;
                if (!_exits[id][step])
                    continue;
                for (int y = 0; y < level.Height; y++)
                    for (int x = 0; x < level.Width; x++)
                    {
                        var cell = new Vector2Int(x, y);
                        if (Has(_low[id][p], _high[id][p], cell))
                            continue;
                        at[1] = cell - probeCell;
                        board.Arrange(at, gone);
                        if (board.CanStep(0, Directions[d]) != StepResult.Exited)
                            Set(ref _wayLow[id][step], ref _wayHigh[id][step], cell);
                    }
            }

            gone[1] = true;
        }

        /// <summary>What every block's blockers allow, for each number of blocks gone, read off the real blocks.</summary>
        void MapBlockers(LevelData level)
        {
            int count = level.Blocks.Count;
            var board = new BoardState(level);
            var at = new Vector2Int[count];
            var gone = new bool[count];
            for (int id = 0; id < count; id++)
            {
                _allows[id] = new bool[count * Directions.Length];
                for (int left = 0; left < count; left++)
                {
                    // Any `left` other blocks gone: the blockers only count them.
                    for (int other = 0, marked = 0; other < count; other++)
                    {
                        gone[other] = other != id && marked < left;
                        if (gone[other])
                            marked++;
                    }

                    board.Arrange(at, gone);
                    for (int d = 0; d < Directions.Length; d++)
                        _allows[id][left * Directions.Length + d] = board.Blocks[id].AllowsStep(Directions[d]);
                }
            }
        }

        static bool FitsAt(LevelData level, BlockData block, Vector2Int offset)
        {
            foreach (Vector2Int cell in block.Cells)
            {
                Vector2Int at = cell + offset;
                if (at.x < 0 || at.y < 0 || at.x >= level.Width || at.y >= level.Height)
                    return false;
            }

            return true;
        }

        static Vector2Int FreeCellOutside(LevelData level, BlockData block)
        {
            for (int y = 0; y < level.Height; y++)
                for (int x = 0; x < level.Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!Contains(block.Cells, cell))
                        return cell;
                }

            return new Vector2Int(-1, -1);
        }

        static bool Contains(IReadOnlyList<Vector2Int> cells, Vector2Int cell)
        {
            for (int i = 0; i < cells.Count; i++)
                if (cells[i] == cell)
                    return true;
            return false;
        }

        int Bit(Vector2Int cell) => cell.y * _width + cell.x;

        void Set(ref ulong low, ref ulong high, Vector2Int cell)
        {
            int bit = Bit(cell);
            if (bit < 64)
                low |= 1UL << bit;
            else
                high |= 1UL << (bit - 64);
        }

        bool Has(ulong low, ulong high, Vector2Int cell)
        {
            int bit = Bit(cell);
            return bit < 64 ? (low & (1UL << bit)) != 0 : (high & (1UL << (bit - 64))) != 0;
        }
    }
}
