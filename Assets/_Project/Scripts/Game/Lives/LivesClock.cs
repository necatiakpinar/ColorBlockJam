using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using VContainer.Unity;

namespace ColorBlockJam.Game.Lives
{
    /// <summary>
    /// Keeps the lives' refill countdown moving for the whole session (Root scope, lives.md): once a second,
    /// through <see cref="IGameClock"/> delays rather than a per-frame update, it gives back lives whose time has
    /// come and announces the countdown while any life is missing.
    /// </summary>
    public sealed class LivesClock : IStartable, IDisposable
    {
        const float TickSeconds = 1f;

        readonly LivesService _lives;
        readonly IGameClock _clock;
        readonly CancellationTokenSource _run = new();

        public LivesClock(LivesService lives, IGameClock clock)
        {
            _lives = lives;
            _clock = clock;
        }

        public void Start() => RunAsync(_run.Token).Forget();

        async UniTaskVoid RunAsync(CancellationToken cancellation)
        {
            while (!await _clock.DelayAsync(TickSeconds, cancellation).SuppressCancellationThrow())
                Tick();
        }

        /// <summary>One second's work; tests call it directly.</summary>
        internal void Tick()
        {
            if (_lives.IsFull)
                return;
            _lives.Refresh();
            _lives.Announce();
        }

        public void Dispose()
        {
            _run.Cancel();
            _run.Dispose();
        }
    }
}
