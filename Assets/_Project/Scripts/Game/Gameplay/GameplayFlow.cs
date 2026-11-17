using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Navigation;
using VContainer.Unity;

namespace ColorBlockJam.Game.Gameplay
{
    /// <summary>
    /// Entry point of the gameplay scene: starts the attempt at the current level through
    /// <see cref="LevelSession"/>, which owns the level's state, moves on to the next level after a win and
    /// routes back to Home. Until the win popup exists (T-21), the next level starts by itself after
    /// <see cref="GameplayConfig.NextLevelDelay"/> (DA-23); the level number has already been advanced by
    /// <see cref="LevelProgression"/> when the win was announced.
    /// </summary>
    public sealed class GameplayFlow : IStartable, IDisposable
    {
        readonly IPlayerProgressService _playerProgress;
        readonly LevelSession _session;
        readonly GameplayHudView _hudView;
        readonly INavigationService _navigation;
        readonly SignalBus _signalBus;
        readonly GameplayConfig _config;
        readonly CancellationTokenSource _lifetime = new();

        public GameplayFlow(IPlayerProgressService playerProgress, LevelSession session, GameplayHudView hudView,
            INavigationService navigation, SignalBus signalBus, GameplayConfig config)
        {
            _playerProgress = playerProgress;
            _session = session;
            _hudView = hudView;
            _navigation = navigation;
            _signalBus = signalBus;
            _config = config;
            _hudView.HomeButtonClicked += OnHomeButtonClicked;
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }

        public void Start() => _session.Start(_playerProgress.CurrentLevel);

        void OnLevelCompleted(LevelCompletedSignal signal) => StartNextLevelAsync(_lifetime.Token).Forget();

        async UniTaskVoid StartNextLevelAsync(CancellationToken cancellation)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_config.NextLevelDelay), DelayType.Realtime,
                cancellationToken: cancellation);
            _session.Next(_playerProgress.CurrentLevel);
        }

        void OnHomeButtonClicked() => _navigation.GoToHomeAsync().Forget();

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _hudView.HomeButtonClicked -= OnHomeButtonClicked;
        }
    }
}
