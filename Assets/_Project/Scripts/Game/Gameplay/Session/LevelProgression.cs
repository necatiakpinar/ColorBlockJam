using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Session
{
    /// <summary>
    /// Moves the player on to the next level as soon as a level is won, before anything presents the win
    /// (T-17). The catalog loops, so after its last level the next number plays the first layout again (DA-8).
    /// </summary>
    public sealed class LevelProgression : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IPlayerProgressService _progress;

        public LevelProgression(SignalBus signalBus, IPlayerProgressService progress)
        {
            _signalBus = signalBus;
            _progress = progress;
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }

        void OnLevelCompleted(LevelCompletedSignal signal) => _progress.AdvanceLevel();

        public void Dispose() => _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
    }
}
