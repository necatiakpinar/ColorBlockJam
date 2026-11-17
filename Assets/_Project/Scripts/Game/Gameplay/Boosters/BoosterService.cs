using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Boosters.Commands;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Boosters
{
    /// <summary>
    /// Uses the boosters during a level (boosters.md, DA-36). Each booster's effect is an
    /// <see cref="IBoosterCommand"/>; this service owns only the shared rules. A command that needs no target runs
    /// at once. One that does first waits: the level pauses (no timer, no dragging) until the player taps a block or
    /// cancels. The booster is taken from the stock before the board changes, and the level resumes first, so
    /// removing the last block wins as any exit does.
    /// </summary>
    public sealed class BoosterService : IDisposable
    {
        readonly ActiveBoard _board;
        readonly LevelSession _session;
        readonly BoosterInventory _inventory;
        readonly SignalBus _signalBus;
        readonly Dictionary<BoosterType, IBoosterCommand> _commands = new();
        IBoosterCommand _pending;

        public BoosterService(ActiveBoard board, LevelSession session, BoosterInventory inventory,
            IReadOnlyList<IBoosterCommand> commands, SignalBus signalBus)
        {
            _board = board;
            _session = session;
            _inventory = inventory;
            _signalBus = signalBus;
            // Add throws on a second command for the same booster: a setup bug that must fail loudly.
            for (int i = 0; i < commands.Count; i++)
                _commands.Add(commands[i].Type, commands[i]);
            _signalBus.Subscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
        }

        /// <summary>A booster is waiting for the player to tap a block.</summary>
        public bool IsTargeting => _pending != null;

        /// <summary>The booster waiting for a target; meaningful only while <see cref="IsTargeting"/>.</summary>
        public BoosterType Targeting => _pending.Type;

        /// <summary>
        /// Starts using a booster: one that needs no target works at once, the others wait for one. Nothing happens
        /// when the player has none left (<see cref="BoosterUse.NeedsPurchase"/>) or the level cannot take it now
        /// (not playing, already choosing a target, or the command refuses).
        /// </summary>
        public BoosterUse Activate(BoosterType type)
        {
            if (_inventory.Count(type) == 0)
                return BoosterUse.NeedsPurchase;
            IBoosterCommand command = CommandFor(type);
            if (IsTargeting || _session.State != LevelState.Playing || !command.CanExecute)
                return BoosterUse.Refused;

            if (!command.NeedsTarget)
            {
                Use(command, BoosterTarget.None);
                return BoosterUse.Used;
            }

            _session.Pause();
            _pending = command;
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

            IBoosterCommand command = _pending;
            EndTargeting();
            _session.Resume();
            Use(command, new BoosterTarget(blockId, signal.Cell));
        }

        void Use(IBoosterCommand command, in BoosterTarget target)
        {
            _inventory.TryUse(command.Type);
            command.Execute(target);
            _signalBus.Fire(new BoosterUsedSignal(command.Type));
        }

        IBoosterCommand CommandFor(BoosterType type) =>
            _commands.TryGetValue(type, out IBoosterCommand command)
                ? command
                : throw new InvalidOperationException($"No command is registered for the {type} booster.");

        /// <summary>A restart drops a target being chosen; the new attempt already plays.</summary>
        void OnLevelStarted(LevelStartedSignal signal)
        {
            if (IsTargeting)
                EndTargeting();
        }

        void EndTargeting()
        {
            BoosterType type = _pending.Type;
            _pending = null;
            _signalBus.Fire(new BoosterTargetingChangedSignal(type, false));
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
        }
    }
}
