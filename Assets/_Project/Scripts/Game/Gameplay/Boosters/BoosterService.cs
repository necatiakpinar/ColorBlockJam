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
            for (int i = 0; i < commands.Count; i++)
                _commands.Add(commands[i].Type, commands[i]);
            _signalBus.Subscribe<BoardPointerSignal>(OnBoardPointer);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
        }

        public bool IsTargeting => _pending != null;

        public BoosterType Targeting => _pending.Type;

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
