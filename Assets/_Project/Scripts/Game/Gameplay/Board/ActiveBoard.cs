using RollicCase.Core.Signals;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Game.Levels;
using UnityEngine;

namespace RollicCase.Game.Gameplay.Board
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
                    _signalBus.Fire(new BlockExitedSignal(blockId, direction));
                    if (Current.IsCleared)
                        _signalBus.Fire(new BoardClearedSignal());
                    break;
            }

            return result;
        }
    }
}
