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
            AnnounceIfCleared();
        }

        /// <summary>
        /// Removes one cell of a block (rocket) and announces <see cref="BlockCellRemovedSignal"/>, or
        /// <see cref="BlockRemovedSignal"/> when it was the block's last cell.
        /// </summary>
        public void RemoveCell(int blockId, Vector2Int cell)
        {
            if (Current.RemoveCell(blockId, cell))
                _signalBus.Fire(new BlockRemovedSignal(blockId, BlockRemovalKind.Blasted, 0));
            else
                _signalBus.Fire(new BlockCellRemovedSignal(blockId, cell));
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
                removed++;
            }

            AnnounceIfCleared();
            return removed;
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
                    if (Current.IsCleared)
                        _signalBus.Fire(new BoardClearedSignal());
                    break;
            }

            return result;
        }
    }
}
