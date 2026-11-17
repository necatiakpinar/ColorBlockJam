using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Session
{
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
