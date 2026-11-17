using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.UI;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using UnityEngine.UI;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelResultFlowTests
    {
        TestViews _views;
        TestCatalog _catalogs;
        SignalBus _bus;
        FakeGameClock _clock;
        RecordingPopupService _popups;
        RecordingNavigation _navigation;
        LevelSession _session;
        PlayerProgressService _progress;
        WalletService _wallet;
        GameplayConfig _config;
        LevelRewardConfig _rewardConfig;
        WinFlow _winFlow;
        LevelWinService _winService;
        RecordingCoinFlight _coinFlight;
        FailFlow _failFlow;
        WinPopupView _winPopup;
        LevelFailedPopupView _levelFailed;
        Button _continueAfterWin;
        Button _retry;
        Button _goHome;
        Button _closeFailed;
        LivesRig _lives;
        LevelAttemptLives _attempt;
        GameplayLivesFlow _livesFlow;
        OutOfLivesPopupView _livesPopup;
        Button _refill;
        Button _livesClose;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            _clock = new FakeGameClock();
            _popups = new RecordingPopupService();
            _navigation = new RecordingNavigation();
            var levels = new LevelProvider(_catalogs.Create(
                new LevelDataBuilder(4, 4).Difficulty(LevelDifficulty.Hard).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build(),
                new LevelDataBuilder(5, 5).Block(Blue, 0, 0).Door(Left, 0, 1, Blue).Build()));
            _session = new LevelSession(new ActiveBoard(new BoardSpace(1f), _bus), levels, _bus);
            _progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            _progress.Load();
            _wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _wallet.Load();
            _config = ScriptableObject.CreateInstance<GameplayConfig>();
            _rewardConfig = ScriptableObject.CreateInstance<LevelRewardConfig>();

            _winPopup = _views.CreateWinPopup(out _continueAfterWin);
            _levelFailed = _views.CreateLevelFailedPopup(out _retry, out _goHome, out _closeFailed);
            _winService = new LevelWinService(_bus, levels, new LevelRewardProvider(_rewardConfig), _progress, _wallet,
                new InMemoryKeyValueStorage());
            _coinFlight = new RecordingCoinFlight();
            _winFlow = new WinFlow(_bus, _popups, _winPopup, _config, _clock, _coinFlight, _navigation);
            _lives = new LivesRig(_bus, _wallet);
            _attempt = new LevelAttemptLives(_lives.Lives, _bus);
            _livesPopup = _views.CreateOutOfLivesPopup(_bus, out _refill, out _livesClose);
            _livesFlow = new GameplayLivesFlow(_lives.Lives, _wallet, _livesPopup, _popups, _session, _navigation);
            _failFlow = new FailFlow(_bus, _popups, _levelFailed, _navigation, _livesFlow);
            _session.Start(1);
        }

        [TearDown]
        public void TearDown()
        {
            _winFlow.Dispose();
            _winService.Dispose();
            _failFlow.Dispose();
            _livesFlow.Dispose();
            _attempt.Dispose();
            _lives.Dispose();
            _session.Dispose();
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_rewardConfig);
            _views.Dispose();
            _catalogs.Dispose();
        }

        void Win() => _bus.Fire(new BoardClearedSignal());

        [Test]
        public void Win_ShouldOpenThePopupWithTheRewardOnlyAfterTheDelay_WhenTheLevelIsWon()
        {
            Win();
            bool openedAtOnce = _popups.IsOpen;

            _clock.Advance(_config.WinPopupDelay);

            Assert.That(openedAtOnce, Is.False, "The last block is seen leaving first.");
            Assert.That(_popups.Current, Is.SameAs(_winPopup));
            Assert.That(_winPopup.RewardText, Is.EqualTo("+25"));
        }

        [Test]
        public void Win_ShouldHoldTheCounterAtTheBalanceBeforeTheReward_WhenTheLevelIsWon()
        {
            Win();

            Assert.That(_wallet.Coins, Is.EqualTo(25), "The wallet is paid at once.");
            Assert.That(_coinFlight.HeldAt, Is.EqualTo(0), "The counter waits for the coins to fly in.");
        }

        [Test]
        public void WinContinue_ShouldFlyTheCoinsThenGoHome_WhenTapped()
        {
            Win();
            _clock.Advance(_config.WinPopupDelay);

            _continueAfterWin.onClick.Invoke();
            bool homeBeforeLanding = _navigation.Requests.Count > 0;
            _coinFlight.Land();
            bool homeBeforeDelay = _navigation.Requests.Count > 0;
            _clock.Advance(_config.CoinFlight.HomeDelay);

            Assert.That(_popups.IsOpen, Is.False);
            Assert.That(_coinFlight.Flights, Is.EqualTo(new[] { 25 }));
            Assert.That(homeBeforeLanding, Is.False, "Home waits for the coins.");
            Assert.That(homeBeforeDelay, Is.False);
            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }), "The next level starts from Home.");
            Assert.That(_session.State, Is.EqualTo(LevelState.Won), "No level starts here.");
        }

        [Test]
        public void WinContinue_ShouldFlyOnce_WhenTappedTwice()
        {
            Win();
            _clock.Advance(_config.WinPopupDelay);

            _continueAfterWin.onClick.Invoke();
            _continueAfterWin.onClick.Invoke();

            Assert.That(_coinFlight.Flights, Has.Count.EqualTo(1));
        }

        [Test]
        public void Win_ShouldNotOpenThePopup_WhenTheFlowIsDisposedDuringTheDelay()
        {
            Win();
            _winFlow.Dispose();

            _clock.Advance(_config.WinPopupDelay);

            Assert.That(_popups.Opened, Is.Empty);
        }

        [Test]
        public void Fail_ShouldOpenLevelFailed_WhenTheTimerRunsOut()
        {
            _session.Fail();

            Assert.That(_popups.Current, Is.SameAs(_levelFailed));
            Assert.That(_session.State, Is.EqualTo(LevelState.Failed));
        }

        [Test]
        public void Fail_ShouldTakeTheAttemptsLife_WhenTheLevelFails()
        {
            _bus.Fire(new BlockGrabbedSignal(0));

            _session.Fail();

            Assert.That(_lives.Lives.Count, Is.EqualTo(_lives.Lives.Max - 1));
        }

        [Test]
        public void Retry_ShouldCloseAndRestartTheLevel_WhenALifeIsLeft()
        {
            _bus.Fire(new BlockGrabbedSignal(0));
            _session.Fail();

            _retry.onClick.Invoke();

            Assert.That(_popups.IsOpen, Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_session.LevelNumber, Is.EqualTo(1));
            Assert.That(_lives.Lives.Count, Is.EqualTo(_lives.Lives.Max - 1), "The fail took the only life it costs.");
        }

        [Test]
        public void Retry_ShouldOfferARefill_WhenNoLivesAreLeft()
        {
            _lives.LoseAll();
            _session.Fail();

            _retry.onClick.Invoke();

            Assert.That(_popups.Current, Is.SameAs(_livesPopup));
            Assert.That(_session.State, Is.EqualTo(LevelState.Failed));
        }

        [Test]
        public void Refill_ShouldRestartTheLevel_WhenBoughtAfterAFail()
        {
            _lives.LoseAll();
            _wallet.Add(_lives.Lives.RefillPrice);
            _session.Fail();
            _retry.onClick.Invoke();

            _refill.onClick.Invoke();

            Assert.That(_popups.IsOpen, Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_lives.Lives.IsFull, Is.True);
        }

        [Test]
        public void ReturnToMenu_ShouldGoHome_WhenTapped()
        {
            _session.Fail();

            _goHome.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void LevelFailedClose_ShouldGoHome_WhenTapped()
        {
            _session.Fail();

            _closeFailed.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }
    }
}
