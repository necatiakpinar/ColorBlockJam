using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Gameplay.View;

namespace ColorBlockJam.Game.Gameplay.Feedback
{
    public sealed class GameplayHaptics : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IHapticService _haptics;
        readonly GameplayConfig _config;
        readonly BoardViewConfig _boardView;
        readonly ISettingsService _settings;
        readonly ActiveBoard _board;
        readonly BoardSpace _boardSpace;
        readonly IGameClock _clock;
        float _trillEndsAt;
        bool _trilling;
        bool _disposed;

        public GameplayHaptics(SignalBus signalBus, IHapticService haptics, GameplayConfig config, BoardViewConfig boardView,
            ISettingsService settings, ActiveBoard board, BoardSpace boardSpace, IGameClock clock)
        {
            _signalBus = signalBus;
            _haptics = haptics;
            _config = config;
            _boardView = boardView;
            _settings = settings;
            _board = board;
            _boardSpace = boardSpace;
            _clock = clock;
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Unsubscribe<BlockExitedSignal>(OnBlockExited);
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
            _disposed = true;
        }

        void OnBlockGrabbed(BlockGrabbedSignal signal) => Tap(_config.GrabHaptic);

        void OnLevelCompleted(LevelCompletedSignal signal) => EndWith(_config.WinHaptic);

        void OnLevelFailed(LevelFailedSignal signal) => EndWith(_config.FailHaptic);

        void EndWith(HapticImpact impact)
        {
            _trillEndsAt = _clock.Now;
            Tap(impact);
        }

        void OnBlockExited(BlockExitedSignal signal)
        {
            var path = new ExitPath(_board.Current.Blocks[signal.BlockId].StartCells, signal.Offset, signal.Direction,
                _boardSpace.Size);
            float endsAt = _clock.Now + _boardView.ExitSnapDuration + _boardView.ExitSlideSeconds(path.Length);
            _trillEndsAt = Math.Max(_trillEndsAt, endsAt);
            if (!_trilling)
                TrillAsync().Forget();
        }

        async UniTaskVoid TrillAsync()
        {
            _trilling = true;
            while (!_disposed && _clock.Now < _trillEndsAt)
            {
                Tap(_config.ExitHaptic);
                await _clock.DelayAsync(_config.ExitTickInterval, CancellationToken.None);
            }

            _trilling = false;
        }

        void Tap(HapticImpact impact)
        {
            if (_settings.IsOn(GameSetting.Vibration))
                _haptics.Play(impact);
        }
    }
}
