using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Signals;

namespace ColorBlockJam.Core.Network
{
    /// <summary>
    /// Probes the internet in a loop from <see cref="Start"/> (DA-25): every
    /// <see cref="ConnectivitySettings.OnlineInterval"/> while online, every
    /// <see cref="ConnectivitySettings.OfflineInterval"/> while offline, and at once when the app comes back
    /// from the background or <see cref="ProbeNowAsync"/> is called. Each successful probe hands its server
    /// time to the <see cref="TrustedClock"/>. Waits are <see cref="IGameClock"/> delays, never per frame.
    /// </summary>
    public sealed class ConnectivityService : IConnectivityService, IDisposable
    {
        readonly IProbeRequest _probe;
        readonly IGameClock _clock;
        readonly TrustedClock _trustedClock;
        readonly SignalBus _signalBus;
        readonly IGameLogger _logger;
        readonly ConnectivitySettings _settings;
        readonly CancellationTokenSource _lifetime = new();
        // Completes the wait between probes early; replaced for every wait.
        UniTaskCompletionSource _wake;
        // Completed with the result of the next probe; created by whoever waits for it.
        UniTaskCompletionSource<bool> _nextResult;

        public ConnectivityService(IProbeRequest probe, IGameClock clock, TrustedClock trustedClock, SignalBus signalBus,
            IGameLogger logger, ConnectivitySettings settings)
        {
            _probe = probe;
            _clock = clock;
            _trustedClock = trustedClock;
            _signalBus = signalBus;
            _logger = logger;
            _settings = settings;
            _signalBus.Subscribe<ApplicationPausedSignal>(OnApplicationPaused);
        }

        public bool IsOnline { get; private set; }
        public bool HasProbed { get; private set; }

        /// <summary>Starts probing. Called once by the Root scope when it is built.</summary>
        public void Start() => RunAsync(_lifetime.Token).Forget();

        public UniTask<bool> ProbeNowAsync()
        {
            _nextResult ??= new UniTaskCompletionSource<bool>();
            UniTask<bool> result = _nextResult.Task;
            _wake?.TrySetResult();
            return result;
        }

        void OnApplicationPaused(ApplicationPausedSignal signal)
        {
            if (!signal.IsPaused)
                _wake?.TrySetResult();
        }

        async UniTaskVoid RunAsync(CancellationToken cancellation)
        {
            try
            {
                while (true)
                {
                    ProbeResult result = await _probe.SendAsync(_settings.ProbeUrl, _settings.ProbeTimeout, cancellation);
                    Apply(result);
                    await WaitForNextProbeAsync(IsOnline ? _settings.OnlineInterval : _settings.OfflineInterval,
                        cancellation);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                // Disposed: the loop ends.
            }
        }

        async UniTask WaitForNextProbeAsync(float seconds, CancellationToken cancellation)
        {
            _wake = new UniTaskCompletionSource();
            using var delay = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            try
            {
                await UniTask.WhenAny(_clock.DelayAsync(seconds, delay.Token), _wake.Task);
            }
            finally
            {
                delay.Cancel();
                _wake = null;
            }

            cancellation.ThrowIfCancellationRequested();
        }

        void Apply(ProbeResult result)
        {
            if (result.Succeeded)
                _trustedClock.TryRecord(result.DateHeader);

            bool changed = !HasProbed || IsOnline != result.Succeeded;
            IsOnline = result.Succeeded;
            HasProbed = true;
            if (changed)
            {
                _logger.Log(IsOnline ? "Connectivity: online" : "Connectivity: offline");
                _signalBus.Fire(new ConnectivityChangedSignal(IsOnline));
            }

            UniTaskCompletionSource<bool> waiting = _nextResult;
            _nextResult = null;
            waiting?.TrySetResult(IsOnline);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ApplicationPausedSignal>(OnApplicationPaused);
            // Cancelled, not disposed: a request still finishing may link to its token afterwards.
            _lifetime.Cancel();
            _nextResult?.TrySetCanceled();
        }
    }
}
