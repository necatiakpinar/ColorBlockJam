using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Input
{
    /// <summary>
    /// Lets a finger slide a block like a solid object (DA-4).
    /// <list type="bullet">
    /// <item>The block always aims at the finger plus the offset it was grabbed at; that offset never
    /// changes, so the block cannot drift away from the finger.</item>
    /// <item>Collisions work like tile physics: each pass moves it along the axis the finger is furthest
    /// along, then the other, as far as every cell its footprint sweeps through is free. It slides along
    /// obstacles and a fast drag cannot pass through anything.</item>
    /// <item>Pushed into a one-cell gap from slightly off the line, it slips sideways into it
    /// (<see cref="BoardViewConfig.CornerTolerance"/>).</item>
    /// <item>Whenever its nearest cell changes, the move is committed to the board, so the board always holds
    /// the cell it settles on. Lined up with a fitting door, the wall stops counting and the block exits once
    /// it is halfway through.</item>
    /// <item>What is drawn may lean toward the finger past an obstacle by
    /// <see cref="BoardViewConfig.HoldGive"/>, so a held block never feels frozen; that lean is never
    /// committed.</item>
    /// </list>
    /// </summary>
    public sealed class BlockDragController : IDisposable
    {
        const int NoBlock = -1;
        const float Epsilon = 1e-4f;
        // One pass moves along both axes; a pass that cannot move ends the event. Bounds the loop.
        const int MaxPassesPerEvent = 16;

        readonly ActiveBoard _board;
        readonly BoardSpace _boardSpace;
        readonly BoardViewConfig _config;
        readonly SignalBus _signalBus;
        int _blockId = NoBlock;
        Vector2 _position;
        Vector2 _grab;
        // Per event: an axis the block just slipped along may not be pulled back the other way.
        Vector2Int _slipLock;

        public BlockDragController(ActiveBoard board, BoardSpace boardSpace, BoardViewConfig config, SignalBus signalBus)
        {
            _board = board;
            _boardSpace = boardSpace;
            _config = config;
            _signalBus = signalBus;
            _signalBus.Subscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
        }

        void OnBoardLoaded(BoardLoadedSignal signal) => _blockId = NoBlock;

        void OnBoardPointer(BoardPointerSignal signal)
        {
            switch (signal.Phase)
            {
                case BoardPointerPhase.Pressed:
                    Grab(signal);
                    break;
                case BoardPointerPhase.Dragged:
                    Vector2 target = Target(signal.WorldPoint);
                    if (Follow(target))
                        _signalBus.Fire(new BlockDraggedSignal(_blockId, _position + Lean(target)));
                    break;
                case BoardPointerPhase.Released:
                    if (Follow(Target(signal.WorldPoint)))
                        _signalBus.Fire(new BlockDroppedSignal(_blockId, CellOffset));
                    _blockId = NoBlock;
                    break;
            }
        }

        void Grab(BoardPointerSignal signal)
        {
            if (!_board.Current.TryGetBlockAt(signal.Cell, out _blockId))
            {
                _blockId = NoBlock;
                return;
            }

            _position = CellOffset;
            _grab = _position - FingerInCells(signal.WorldPoint);
            _signalBus.Fire(new BlockGrabbedSignal(_blockId));
        }

        Vector2 Target(Vector3 fingerWorld) => FingerInCells(fingerWorld) + _grab;

        /// <summary>The drawn-only lean toward the finger, at most <see cref="BoardViewConfig.HoldGive"/>.</summary>
        Vector2 Lean(Vector2 target) => Vector2.ClampMagnitude(target - _position, _config.HoldGive);

        /// <summary>Moves the held block toward the target; false when no block is held (any more).</summary>
        bool Follow(Vector2 target)
        {
            if (_blockId == NoBlock)
                return false;

            _slipLock = Vector2Int.zero;
            for (int pass = 0; pass < MaxPassesPerEvent && _blockId != NoBlock; pass++)
            {
                Vector2 remaining = target - _position;
                int first = Mathf.Abs(remaining.x) >= Mathf.Abs(remaining.y) ? 0 : 1;
                bool moved = Slide(first, target[first]);
                if (_blockId != NoBlock)
                    moved |= Slide(1 - first, target[1 - first]);
                if (!moved)
                    break;
            }

            return _blockId != NoBlock;
        }

        /// <summary>Slides along one axis toward <paramref name="goal"/> as far as the way is free.</summary>
        bool Slide(int axis, float goal)
        {
            float wanted = goal - _position[axis];
            if (Mathf.Abs(wanted) <= Epsilon)
                return false;

            int direction = wanted > 0f ? 1 : -1;
            if (_slipLock[axis] == -direction)
                return false;

            float free = FreeDistance(axis, direction, Mathf.Abs(wanted));
            bool moved = false;
            if (free > Epsilon)
            {
                _position[axis] += direction * free;
                Commit(axis, direction);
                moved = true;
            }

            float blocked = Mathf.Abs(wanted) - free;
            if (_blockId != NoBlock && blocked > Epsilon)
                moved |= SlipIntoGap(axis, direction, blocked);
            return moved;
        }

        /// <summary>
        /// When a push is stopped only because the block is slightly off the cell line on the other axis (it
        /// straddles a free and a blocked row), turns the stopped part of the push into a sideways slip onto
        /// the nearest line, if that line opens the way. Nothing happens otherwise.
        /// </summary>
        bool SlipIntoGap(int axis, int direction, float blocked)
        {
            int cross = 1 - axis;
            float current = _position[cross];
            float line = Mathf.Round(current);
            float offLine = Mathf.Abs(line - current);
            if (offLine <= Epsilon || offLine > _config.CornerTolerance)
                return false;

            float freeHere = FreeDistance(axis, direction, blocked);
            _position[cross] = line;
            float freeOnLine = FreeDistance(axis, direction, blocked);
            _position[cross] = current;
            if (freeOnLine <= freeHere + Epsilon)
                return false;

            int sideways = line > current ? 1 : -1;
            float slip = FreeDistance(cross, sideways, Mathf.Min(blocked, offLine));
            if (slip <= Epsilon)
                return false;

            _position[cross] += sideways * slip;
            _slipLock[cross] = sideways;
            Commit(cross, sideways);
            return true;
        }

        /// <summary>
        /// How far the block can move along <paramref name="axis"/>, up to <paramref name="wanted"/>. Each of
        /// its cells is a unit square that, when between cells, spans two rows (or columns); the scan walks the
        /// cells ahead of its leading edge until one is blocked or the wanted distance is reached.
        /// </summary>
        float FreeDistance(int axis, int direction, float wanted)
        {
            BoardState board = _board.Current;
            BoardBlock block = board.Blocks[_blockId];
            int cross = 1 - axis;
            bool doorOpen = IsAligned(cross) && board.CanStep(_blockId, Step(axis, direction)) == StepResult.Exited;

            float free = wanted;
            for (int i = 0; i < block.CellCount; i++)
            {
                Vector2Int start = block.GetCell(i) - block.Offset;
                float near = start[axis] + _position[axis];
                float edge = direction > 0 ? near + 1f : near;
                float crossPosition = start[cross] + _position[cross];
                int firstRow = Mathf.FloorToInt(crossPosition + Epsilon);
                int lastRow = Mathf.CeilToInt(crossPosition - Epsilon);

                int column = direction > 0 ? Mathf.CeilToInt(edge - Epsilon) : Mathf.FloorToInt(edge + Epsilon) - 1;
                while (true)
                {
                    float reach = direction > 0 ? column - edge : edge - (column + 1);
                    if (reach >= free)
                        break;
                    if (IsBlocked(axis, column, firstRow, lastRow, doorOpen))
                    {
                        free = Mathf.Max(0f, reach);
                        break;
                    }

                    column += direction;
                }
            }

            return free;
        }

        bool IsBlocked(int axis, int column, int firstRow, int lastRow, bool doorOpen)
        {
            BoardState board = _board.Current;
            for (int row = firstRow; row <= lastRow; row++)
            {
                Vector2Int cell = axis == 0 ? new Vector2Int(column, row) : new Vector2Int(row, column);
                if (board.IsCellFree(cell, _blockId))
                    continue;
                // Off the board only through the fitting door the block is lined up with.
                bool offBoard = cell.x < 0 || cell.y < 0 || cell.x >= board.Width || cell.y >= board.Height;
                if (doorOpen && offBoard)
                    continue;
                return true;
            }

            return false;
        }

        /// <summary>Steps the board block until it sits on the cell nearest to where it is drawn.</summary>
        void Commit(int axis, int direction)
        {
            int nearest = Mathf.RoundToInt(_position[axis]);
            Vector2Int step = Step(axis, direction);
            while (_blockId != NoBlock && CellOffset[axis] != nearest)
            {
                StepResult result = _board.TryStep(_blockId, step);
                if (result == StepResult.Exited)
                    _blockId = NoBlock;
                else if (result == StepResult.Blocked)
                    break;
            }
        }

        bool IsAligned(int axis) => Mathf.Abs(_position[axis] - Mathf.Round(_position[axis])) <= Epsilon;

        Vector2Int Step(int axis, int direction) =>
            axis == 0 ? new Vector2Int(direction, 0) : new Vector2Int(0, direction);

        Vector2Int CellOffset => _board.Current.Blocks[_blockId].Offset;

        Vector2 FingerInCells(Vector3 world) => new Vector2(world.x, world.z) / _boardSpace.CellSize;

        public void Dispose()
        {
            _signalBus.Unsubscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
        }
    }
}
