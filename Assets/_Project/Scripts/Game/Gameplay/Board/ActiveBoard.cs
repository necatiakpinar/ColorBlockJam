using System.Collections.Generic;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board.Blockers;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using Unity.Profiling;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    public sealed class ActiveBoard
    {
        static readonly ProfilerMarker MoveMarker = new("ColorBlockJam.Move");

        readonly BoardSpace _boardSpace;
        readonly SignalBus _signalBus;

        public ActiveBoard(BoardSpace boardSpace, SignalBus signalBus)
        {
            _boardSpace = boardSpace;
            _signalBus = signalBus;
        }

        public BoardState Current { get; private set; }

        public void Load(LevelData level)
        {
            Current = new BoardState(level);
            _boardSpace.SetSize(new Vector2Int(level.Width, level.Height));
            _signalBus.Fire(new BoardLoadedSignal(level));
        }

        public void RemoveBlock(int blockId)
        {
            Current.RemoveBlock(blockId);
            _signalBus.Fire(new BlockRemovedSignal(blockId, BlockRemovalKind.Smashed, 0));
            AnnounceThaws();
            AnnounceIfCleared();
        }

        public void Thaw(int blockId)
        {
            Current.Thaw(blockId);
            _signalBus.Fire(new BlockThawedSignal(blockId));
        }

        public void RemoveCell(int blockId, Vector2Int cell)
        {
            if (Current.RemoveCell(blockId, cell))
            {
                _signalBus.Fire(new BlockRemovedSignal(blockId, BlockRemovalKind.Blasted, 0));
                AnnounceThaws();
            }
            else
            {
                _signalBus.Fire(new BlockCellRemovedSignal(blockId, cell));
                IReadOnlyList<int> pieces = Current.JustSplitOff;
                for (int i = 0; i < pieces.Count; i++)
                    _signalBus.Fire(new BlockSplitOffSignal(pieces[i], blockId));
            }

            AnnounceIfCleared();
        }

        public int RemoveColor(BlockColor color)
        {
            int removed = 0;
            for (int id = 0; id < Current.Blocks.Count; id++)
            {
                BoardBlock block = Current.Blocks[id];
                if (block.HasExited || block.Color != color)
                    continue;
                Current.RemoveBlock(id);
                _signalBus.Fire(new BlockRemovedSignal(id, BlockRemovalKind.Vacuumed, removed));
                AnnounceThaws();
                removed++;
            }

            AnnounceIfCleared();
            return removed;
        }

        void AnnounceThaws()
        {
            if (!HasIce())
                return;
            _signalBus.Fire(new IceCountedDownSignal());
            IReadOnlyList<int> thawed = Current.JustThawed;
            for (int i = 0; i < thawed.Count; i++)
                _signalBus.Fire(new BlockThawedSignal(thawed[i]));
        }

        bool HasIce()
        {
            if (Current.JustThawed.Count > 0)
                return true;
            foreach (BoardBlock block in Current.Blocks)
                if (!block.HasExited && block.TryGetBlocker(out IceBlocker ice) && ice.IsFrozen)
                    return true;
            return false;
        }

        void AnnounceIfCleared()
        {
            if (Current.IsCleared)
                _signalBus.Fire(new BoardClearedSignal());
        }

        public StepResult TryStep(int blockId, Vector2Int direction)
        {
            using ProfilerMarker.AutoScope scope = MoveMarker.Auto();
            StepResult result = Current.TryStep(blockId, direction);
            switch (result)
            {
                case StepResult.Moved:
                    _signalBus.Fire(new BlockMovedSignal(blockId, Current.Blocks[blockId].Offset));
                    break;
                case StepResult.Exited:
                    _signalBus.Fire(new BlockExitedSignal(blockId, direction, Current.Blocks[blockId].Offset));
                    AnnounceThaws();
                    AnnounceIfCleared();
                    break;
            }

            return result;
        }
    }
}
