using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Boosters
{
    /// <summary>
    /// Uses the boosters during a level (boosters.md, DA-36). The freeze works at once. The rocket, the hammer and
    /// the vacuum first wait for a target: the level pauses (no timer, no dragging) until the player taps a block
    /// or cancels. The booster is taken from the stock before the board changes, and the level resumes first, so
    /// removing the last block wins as any exit does.
    /// </summary>
    public sealed class BoosterService : IDisposable
    {
        readonly ActiveBoard _board;
        readonly LevelSession _session;
        readonly LevelTimer _timer;
        readonly BoosterInventory _inventory;
        readonly BoosterConfig _config;
        readonly SignalBus _signalBus;

        public BoosterService(ActiveBoard board, LevelSession session, LevelTimer timer, BoosterInventory inventory,
            BoosterConfig config, SignalBus signalBus)
        {
            _board = board;
            _session = session;
            _timer = timer;
            _inventory = inventory;
            _config = config;
            _signalBus = signalBus;
            _signalBus.Subscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
        }

        /// <summary>A rocket, hammer or vacuum is waiting for the player to tap a block.</summary>
        public bool IsTargeting { get; private set; }

        /// <summary>The booster waiting for a target; meaningful only while <see cref="IsTargeting"/>.</summary>
        public BoosterType Targeting { get; private set; }

        /// <summary>
        /// Starts using a booster: the freeze works at once, the others wait for a target. Nothing happens when
        /// the player has none left (<see cref="BoosterUse.NeedsPurchase"/>) or the level cannot take it now
        /// (not playing, already frozen, already choosing a target).
        /// </summary>
        public BoosterUse Activate(BoosterType type)
        {
            if (_inventory.Count(type) == 0)
                return BoosterUse.NeedsPurchase;
            if (IsTargeting || _session.State != LevelState.Playing)
                return BoosterUse.Refused;

            if (type == BoosterType.Freeze)
            {
                if (_timer.IsFrozen)
                    return BoosterUse.Refused;
                _inventory.TryUse(type);
                _timer.FreezeFor(_config.FreezeSeconds);
                _signalBus.Fire(new BoosterUsedSignal(type));
                return BoosterUse.Used;
            }

            _session.Pause();
            IsTargeting = true;
            Targeting = type;
            _signalBus.Fire(new BoosterTargetingChangedSignal(type, true));
            return BoosterUse.Targeting;
        }

        /// <summary>Gives up choosing a target: the booster is kept and the level plays on.</summary>
        public void CancelTargeting()
        {
            if (!IsTargeting)
                return;
            EndTargeting();
            _session.Resume();
        }

        void OnBoardPointer(BoardPointerSignal signal)
        {
            if (!IsTargeting || signal.Phase != BoardPointerPhase.Pressed
                || !_board.Current.TryGetBlockAt(signal.Cell, out int blockId))
                return;

            BoosterType type = Targeting;
            EndTargeting();
            _session.Resume();
            _inventory.TryUse(type);
            switch (type)
            {
                case BoosterType.Rocket:
                    _board.RemoveCell(blockId, signal.Cell);
                    break;
                case BoosterType.Hammer:
                    // On a frozen block the hammer breaks the ice instead (DA-37).
                    if (_board.Current.Blocks[blockId].IsFrozen)
                        _board.Thaw(blockId);
                    else
                        _board.RemoveBlock(blockId);
                    break;
                case BoosterType.Vacuum:
                    _board.RemoveColor(_board.Current.Blocks[blockId].Color);
                    break;
                default:
                    throw new InvalidOperationException($"The {type} booster takes no target.");
            }

            _signalBus.Fire(new BoosterUsedSignal(type));
        }

        /// <summary>A restart drops a target being chosen; the new attempt already plays.</summary>
        void OnLevelStarted(LevelStartedSignal signal)
        {
            if (IsTargeting)
                EndTargeting();
        }

        void EndTargeting()
        {
            IsTargeting = false;
            _signalBus.Fire(new BoosterTargetingChangedSignal(Targeting, false));
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
        }
    }
}
