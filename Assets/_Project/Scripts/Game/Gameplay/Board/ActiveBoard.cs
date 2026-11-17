using System.Collections.Generic;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>
    /// Owns the board of the current attempt and announces what happens to it. Every load builds a new
    /// <see cref="BoardState"/>; every step goes through here so moves, exits and the clear are announced
    /// in one place, whoever asked for the step.
    /// </summary>
    public sealed class ActiveBoard
    {
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

        /// <summary>Removes a whole block (hammer) and announces it; the last one clears the board.</summary>
        public void RemoveBlock(int blockId)
        {
            Current.RemoveBlock(blockId);
            _signalBus.Fire(new BlockRemovedSignal(blockId, BlockRemovalKind.Smashed, 0));
            AnnounceThaws();
            AnnounceIfCleared();
        }

        /// <summary>Breaks a frozen block's ice at once (the hammer, DA-37) and announces it.</summary>
        public void Thaw(int blockId)
        {
            Current.Thaw(blockId);
            _signalBus.Fire(new BlockThawedSignal(blockId));
        }

        /// <summary>
        /// Removes one cell of a block (rocket) and announces <see cref="BlockCellRemovedSignal"/>, then a
        /// <see cref="BlockSplitOffSignal"/> for each piece that became a block of its own; or
        /// <see cref="BlockRemovedSignal"/> when it was the block's last cell.
        /// </summary>
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

        /// <summary>Removes every block of <paramref name="color"/> on the board (vacuum), in id order.</summary>
        /// <returns>How many blocks went.</returns>
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

        /// <summary>
        /// After a block left: <see cref="IceCountedDownSignal"/> when frozen blocks counted it, then
        /// <see cref="BlockThawedSignal"/> for each block it thawed.
        /// </summary>
        void AnnounceThaws()
        {
            if (!HasIce())
                return;
            _signalBus.Fire(new IceCountedDownSignal());
            IReadOnlyList<int> thawed = Current.JustThawed;
            for (int i = 0; i < thawed.Count; i++)
                _signalBus.Fire(new BlockThawedSignal(thawed[i]));
        }

        /// <summary>True when some block on the board is frozen or was just thawed.</summary>
        bool HasIce()
        {
            if (Current.JustThawed.Count > 0)
                return true;
            foreach (BoardBlock block in Current.Blocks)
                if (!block.HasExited && block.IsFrozen)
                    return true;
            return false;
        }

        void AnnounceIfCleared()
        {
            if (Current.IsCleared)
                _signalBus.Fire(new BoardClearedSignal());
        }

        /// <summary>
        /// Pushes a block one cell (<see cref="BoardState.TryStep"/>) and announces the outcome:
        /// <see cref="BlockMovedSignal"/>, or <see cref="BlockExitedSignal"/> followed by
        /// <see cref="BoardClearedSignal"/> when it was the last block.
        /// </summary>
        public StepResult TryStep(int blockId, Vector2Int direction)
        {
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
