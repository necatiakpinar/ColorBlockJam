using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Session
{
    public sealed class LevelTimer : IDisposable
    {
        readonly LevelSession _session;
        readonly IGameClock _clock;
        readonly SignalBus _signalBus;
        int _fullSeconds;
        float _remaining;
        float _runStart;
        bool _levelPlaying;
        bool _appPaused;
        bool _touched;
        int _shown = -1;
        CancellationTokenSource _run;
        float _freezeTotal;
        float _freezeLeft;
        float _freezeStart;
        CancellationTokenSource _freezeRun;

        public LevelTimer(LevelSession session, IGameClock clock, SignalBus signalBus)
        {
            _session = session;
            _clock = clock;
            _signalBus = signalBus;
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<LevelPausedSignal>(OnLevelPaused);
            _signalBus.Subscribe<LevelResumedSignal>(OnLevelResumed);
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
            _signalBus.Subscribe<ApplicationPausedSignal>(OnApplicationPaused);
        }

        bool IsRunning => _run != null;

        public float SecondsLeft => Mathf.Max(0f, IsRunning ? _remaining - (_clock.Now - _runStart) : _remaining);

        public bool IsFrozen => _freezeLeft > 0f;

        public float FreezeSecondsLeft =>
            Mathf.Max(0f, _freezeRun != null ? _freezeLeft - (_clock.Now - _freezeStart) : _freezeLeft);

        public bool FreezeFor(float seconds)
        {
            if (IsFrozen || seconds <= 0f)
                return false;
            _freezeTotal = seconds;
            _freezeLeft = seconds;
            AnnounceFreeze(false);
            Refresh();
            return true;
        }

        public void SetSecondsLeft(float seconds)
        {
            StopCounting();
            _remaining = Mathf.Max(0f, seconds);
            Announce();
            Refresh();
        }

        void OnBoardLoaded(BoardLoadedSignal signal) => _fullSeconds = signal.Level.TimerSeconds;

        void OnLevelStarted(LevelStartedSignal signal)
        {
            StopCounting();
            ClearFreeze();
            _remaining = _fullSeconds;
            _shown = -1;
            _touched = false;
            Announce();
            SetLevelPlaying(true);
        }

        void OnBlockGrabbed(BlockGrabbedSignal signal)
        {
            _touched = true;
            Refresh();
        }

        void OnLevelPaused(LevelPausedSignal signal) => SetLevelPlaying(false);

        void OnLevelResumed(LevelResumedSignal signal) => SetLevelPlaying(true);

        void OnLevelCompleted(LevelCompletedSignal signal) => SetLevelPlaying(false);

        void OnLevelFailed(LevelFailedSignal signal) => SetLevelPlaying(false);

        void OnApplicationPaused(ApplicationPausedSignal signal)
        {
            _appPaused = signal.IsPaused;
            Refresh();
        }

        void SetLevelPlaying(bool playing)
        {
            _levelPlaying = playing;
            Refresh();
        }

        void Refresh()
        {
            bool counting = _levelPlaying && !_appPaused && _touched;
            if (IsFrozen && counting && _freezeRun == null)
                StartFreezeRun();
            else if (_freezeRun != null && !counting)
                StopFreezeRun();

            bool shouldRun = counting && !IsFrozen;
            if (shouldRun && !IsRunning)
            {
                _runStart = _clock.Now;
                _run = new CancellationTokenSource();
                RunAsync(_run.Token).Forget();
            }
            else if (!shouldRun)
            {
                StopCounting();
            }
        }

        void StopCounting()
        {
            if (!IsRunning)
                return;
            _remaining = SecondsLeft;
            _run.Cancel();
            _run.Dispose();
            _run = null;
        }

        void StartFreezeRun()
        {
            _freezeStart = _clock.Now;
            _freezeRun = new CancellationTokenSource();
            RunFreezeAsync(_freezeRun.Token).Forget();
            AnnounceFreeze(true);
        }

        void StopFreezeRun()
        {
            _freezeLeft = FreezeSecondsLeft;
            _freezeRun.Cancel();
            _freezeRun.Dispose();
            _freezeRun = null;
            AnnounceFreeze(false);
        }

        async UniTaskVoid RunFreezeAsync(CancellationToken cancellation)
        {
            if (await _clock.DelayAsync(_freezeLeft, cancellation).SuppressCancellationThrow())
                return;
            _freezeRun.Dispose();
            _freezeRun = null;
            _freezeLeft = 0f;
            AnnounceFreeze(false);
            Refresh();
        }

        void ClearFreeze()
        {
            if (_freezeRun != null)
            {
                _freezeRun.Cancel();
                _freezeRun.Dispose();
                _freezeRun = null;
            }

            if (!IsFrozen)
                return;
            _freezeLeft = 0f;
            AnnounceFreeze(false);
        }

        void AnnounceFreeze(bool counting) =>
            _signalBus.Fire(new TimerFreezeChangedSignal(FreezeSecondsLeft, _freezeTotal, counting));

        async UniTaskVoid RunAsync(CancellationToken cancellation)
        {
            while (true)
            {
                float left = SecondsLeft;
                if (left <= 0f)
                {
                    Expire();
                    return;
                }

                float untilNextSecond = left - (Mathf.Ceil(left) - 1f);
                if (await _clock.DelayAsync(untilNextSecond, cancellation).SuppressCancellationThrow())
                    return;
                Announce();
            }
        }

        void Expire()
        {
            StopCounting();
            _remaining = 0f;
            Announce();
            _session.Fail();
        }

        void Announce()
        {
            int shown = Mathf.CeilToInt(SecondsLeft);
            if (shown == _shown)
                return;
            _shown = shown;
            _signalBus.Fire(new TimerChangedSignal(shown));
        }

        public void Dispose()
        {
            StopCounting();
            _freezeRun?.Cancel();
            _freezeRun?.Dispose();
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Unsubscribe<LevelPausedSignal>(OnLevelPaused);
            _signalBus.Unsubscribe<LevelResumedSignal>(OnLevelResumed);
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
            _signalBus.Unsubscribe<ApplicationPausedSignal>(OnApplicationPaused);
        }
    }
}
