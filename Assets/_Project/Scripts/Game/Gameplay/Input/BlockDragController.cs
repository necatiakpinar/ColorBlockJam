using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using Unity.Profiling;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Input
{
    public sealed class BlockDragController : IDisposable
    {
        static readonly ProfilerMarker DragMarker = new("ColorBlockJam.Drag");

        const int NoBlock = -1;
        const float Epsilon = 1e-4f;
        const int MaxPassesPerEvent = 16;

        readonly ActiveBoard _board;
        readonly BoardSpace _boardSpace;
        readonly BoardViewConfig _config;
        readonly SignalBus _signalBus;
        int _blockId = NoBlock;
        bool _acceptsInput = true;
        bool _boosterTargeting;
        Vector2 _position;
        Vector2 _grab;
        Vector2 _target;
        Vector2Int _slipLock;
        Vector2Int _lastSlip;

        public BlockDragController(ActiveBoard board, BoardSpace boardSpace, BoardViewConfig config, SignalBus signalBus)
        {
            _board = board;
            _boardSpace = boardSpace;
            _config = config;
            _signalBus = signalBus;
            _signalBus.Subscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<LevelResumedSignal>(OnLevelResumed);
            _signalBus.Subscribe<LevelPausedSignal>(OnLevelPaused);
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
            _signalBus.Subscribe<BoosterTargetingChangedSignal>(OnBoosterTargetingChanged);
        }

        void OnBoardLoaded(BoardLoadedSignal signal) => _blockId = NoBlock;

        void OnLevelStarted(LevelStartedSignal signal) => _acceptsInput = true;

        void OnLevelResumed(LevelResumedSignal signal) => _acceptsInput = true;

        void OnLevelPaused(LevelPausedSignal signal) => StopInput();

        void OnLevelCompleted(LevelCompletedSignal signal) => StopInput();

        void OnLevelFailed(LevelFailedSignal signal) => StopInput();

        void OnBoosterTargetingChanged(BoosterTargetingChangedSignal signal)
        {
            _boosterTargeting = signal.Active;
            if (signal.Active)
                DropHeldBlock();
        }

        void StopInput()
        {
            _acceptsInput = false;
            DropHeldBlock();
        }

        void DropHeldBlock()
        {
            if (_blockId == NoBlock)
                return;
            if (!_board.Current.Blocks[_blockId].HasExited)
                _signalBus.Fire(new BlockDroppedSignal(_blockId, CellOffset));
            _blockId = NoBlock;
        }

        void OnBoardPointer(BoardPointerSignal signal)
        {
            if (!_acceptsInput || _boosterTargeting)
                return;
            using ProfilerMarker.AutoScope scope = DragMarker.Auto();
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
                case BoardPointerPhase.Cancelled:
                    DropHeldBlock();
                    break;
            }
        }

        void Grab(BoardPointerSignal signal)
        {
            if ((!_board.Current.TryGetBlockAt(signal.Cell, out _blockId)
                    && !TryGetNearestBlock(signal.WorldPoint, out _blockId))
                || !_board.Current.Blocks[_blockId].CanBeGrabbed)
            {
                _blockId = NoBlock;
                return;
            }

            _position = CellOffset;
            _grab = _position - FingerInCells(signal.WorldPoint);
            _lastSlip = Vector2Int.zero;
            _signalBus.Fire(new BlockGrabbedSignal(_blockId));
        }

        bool TryGetNearestBlock(Vector3 fingerWorld, out int blockId)
        {
            blockId = NoBlock;
            Vector3 local = (fingerWorld - _boardSpace.Origin) / _boardSpace.CellSize;
            var finger = new Vector2(local.x, local.z);
            var under = new Vector2Int(Mathf.FloorToInt(finger.x), Mathf.FloorToInt(finger.y));
            float best = _config.TouchTolerance;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int cell = under + new Vector2Int(dx, dy);
                    if (!_board.Current.TryGetBlockAt(cell, out int candidate))
                        continue;
                    float gapX = Mathf.Max(cell.x - finger.x, 0f, finger.x - (cell.x + 1));
                    float gapY = Mathf.Max(cell.y - finger.y, 0f, finger.y - (cell.y + 1));
                    float distance = Mathf.Sqrt(gapX * gapX + gapY * gapY);
                    if (distance <= best)
                    {
                        best = distance;
                        blockId = candidate;
                    }
                }
            }

            return blockId != NoBlock;
        }

        Vector2 Target(Vector3 fingerWorld) => FingerInCells(fingerWorld) + _grab;

        Vector2 Lean(Vector2 target)
        {
            Vector2 lean = target - _position;
            BoardBlock block = _board.Current.Blocks[_blockId];
            if (!block.AllowsStep(new Vector2Int(lean.x >= 0f ? 1 : -1, 0)))
                lean.x = 0f;
            if (!block.AllowsStep(new Vector2Int(0, lean.y >= 0f ? 1 : -1)))
                lean.y = 0f;
            return Vector2.ClampMagnitude(lean, _config.HoldGive);
        }

        bool Follow(Vector2 target)
        {
            if (_blockId == NoBlock)
                return false;

            _slipLock = Vector2Int.zero;
            _target = target;
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

        bool Slide(int axis, float goal)
        {
            float wanted = goal - _position[axis];
            if (Mathf.Abs(wanted) <= Epsilon)
                return false;

            int direction = wanted > 0f ? 1 : -1;
            if (_slipLock[axis] == -direction || HoldsLine(axis, direction, Mathf.Abs(wanted)))
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
            if (_blockId != NoBlock && blocked > Epsilon && !UndoesSlip(axis, direction, Mathf.Abs(wanted)))
                moved |= SlipIntoGap(axis, direction, blocked);
            return moved;
        }

        bool HoldsLine(int axis, int direction, float wanted)
        {
            int other = 1 - axis;
            float push = _target[other] - _position[other];
            int pushDirection = push > 0f ? 1 : -1;
            if (Mathf.Abs(push) <= Epsilon || Mathf.Abs(push) < wanted
                || UndoesSlip(other, pushDirection, Mathf.Abs(push)))
                return false;

            float current = _position[axis];
            float line = Mathf.Round(current);
            if (Mathf.Abs(line - current) > _config.CornerTolerance || (current - line) * direction < -Epsilon)
                return false;

            float reach = Mathf.Min(Mathf.Abs(push), 1f);
            _position[axis] = line;
            float freeOnLine = FreeDistance(other, pushDirection, reach);
            _position[axis] = current + direction * FreeDistance(axis, direction, wanted);
            float freeAfter = FreeDistance(other, pushDirection, reach);
            _position[axis] = current;
            return freeOnLine > freeAfter + Epsilon;
        }

        bool UndoesSlip(int axis, int direction, float wanted) =>
            _lastSlip[axis] == -direction && wanted <= _config.CornerTolerance;

        bool SlipIntoGap(int axis, int direction, float blocked)
        {
            int cross = 1 - axis;
            if (blocked < Mathf.Abs(_target[cross] - _position[cross]))
                return false;

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
            _lastSlip[cross] = sideways;
            Commit(cross, sideways);
            return true;
        }

        float FreeDistance(int axis, int direction, float wanted)
        {
            BoardState board = _board.Current;
            BoardBlock block = board.Blocks[_blockId];
            if (!block.AllowsStep(Step(axis, direction)))
                return 0f;
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
                bool offBoard = cell.x < 0 || cell.y < 0 || cell.x >= board.Width || cell.y >= board.Height;
                if (doorOpen && offBoard)
                    continue;
                return true;
            }

            return false;
        }

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

            if (_blockId != NoBlock && (_position[axis] - CellOffset[axis]) * direction >= _config.ExitPush
                && _board.Current.CanStep(_blockId, step) == StepResult.Exited)
            {
                _board.TryStep(_blockId, step);
                _blockId = NoBlock;
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
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<LevelResumedSignal>(OnLevelResumed);
            _signalBus.Unsubscribe<LevelPausedSignal>(OnLevelPaused);
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Unsubscribe<BoosterTargetingChangedSignal>(OnBoosterTargetingChanged);
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
        }
    }
}
