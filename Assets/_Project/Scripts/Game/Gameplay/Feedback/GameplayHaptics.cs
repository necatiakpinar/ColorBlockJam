using System;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Feedback
{
    /// <summary>
    /// Turns board events into haptic taps (DA-21): a firmer one when a push is refused by a door, a light
    /// one when a block leaves through its door. Strengths come from <see cref="GameplayConfig"/>.
    /// </summary>
    public sealed class GameplayHaptics : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IHapticService _haptics;
        readonly GameplayConfig _config;

        public GameplayHaptics(SignalBus signalBus, IHapticService haptics, GameplayConfig config)
        {
            _signalBus = signalBus;
            _haptics = haptics;
            _config = config;
            _signalBus.Subscribe<BlockRejectedSignal>(OnBlockRejected);
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
        }

        void OnBlockRejected(BlockRejectedSignal signal) => _haptics.Play(_config.RejectHaptic);

        void OnBlockExited(BlockExitedSignal signal) => _haptics.Play(_config.ExitHaptic);

        public void Dispose()
        {
            _signalBus.Unsubscribe<BlockRejectedSignal>(OnBlockRejected);
            _signalBus.Unsubscribe<BlockExitedSignal>(OnBlockExited);
        }
    }
}
