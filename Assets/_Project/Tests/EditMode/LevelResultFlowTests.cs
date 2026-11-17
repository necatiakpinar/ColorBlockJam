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
    /// <summary>The win popup, and the out-of-time and level-failed popups after a loss (gameplay.md §7).</summary>
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
        FailFlow _failFlow;
        WinPopupView _winPopup;
        OutOfTimePopupView _outOfTime;
        LevelFailedPopupView _levelFailed;
        Button _continueAfterWin;
        Button _buyTime;
        Button _giveUp;
        Button _retry;
        Button _goHome;
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
            // Level 1 is Hard (25 coins); level 2 is Normal.
            var levels = new LevelProvider(_catalogs.Create(
                new LevelDataBuilder(4, 4).Difficulty(LevelDifficulty.Hard).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build(),
                new LevelDataBuilder(5, 5).Block(Blue, 0, 0).Door(Left, 0, 1, Blue).Build()));
            _session = new LevelSession(new ActiveBoard(new BoardSpace(1f), _bus), levels, _bus);
            _progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            _progress.Load();
            _wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _wallet.Load();
            // Defaults: popup 1 s after a win; 20 more seconds for 50 coins.
            _config = ScriptableObject.CreateInstance<GameplayConfig>();
            _rewardConfig = ScriptableObject.CreateInstance<LevelRewardConfig>();

            _winPopup = _views.CreateWinPopup(out _continueAfterWin);
            _outOfTime = _views.CreateOutOfTimePopup(out _buyTime, out _giveUp);
            _levelFailed = _views.CreateLevelFailedPopup(out _retry, out _goHome);
            _winFlow = new WinFlow(_bus, _popups, _winPopup, _session, _progress, levels,
                new LevelRewardProvider(_rewardConfig), _config, _clock);
            _lives = new LivesRig(_bus, _wallet);
            _attempt = new LevelAttemptLives(_lives.Lives, _bus);
            _livesPopup = _views.CreateOutOfLivesPopup(_bus, out _refill, out _livesClose);
            _livesFlow = new GameplayLivesFlow(_lives.Lives, _wallet, _livesPopup, _popups, _session, _navigation);
            _failFlow = new FailFlow(_bus, _popups, _outOfTime, _levelFailed, _session,
                new LevelContinueService(_session, _wallet, _config), _navigation, _attempt, _livesFlow);
            _session.Start(1);
        }

        [TearDown]
        public void TearDown()
        {
            _winFlow.Dispose();
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

        void Win()
        {
            _bus.Fire(new BoardClearedSignal());
            _progress.AdvanceLevel();
        }

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
        public void WinContinue_ShouldCloseAndStartTheNextLevel_WhenTapped()
        {
            Win();
            _clock.Advance(_config.WinPopupDelay);
            int started = 0;
            _bus.Subscribe<LevelStartedSignal>(signal => started = signal.LevelNumber);

            _continueAfterWin.onClick.Invoke();

            Assert.That(_popups.IsOpen, Is.False);
            Assert.That(started, Is.EqualTo(2));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
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
        public void Fail_ShouldOfferMoreTimeForCoins_WhenTheTimerRunsOut()
        {
            _wallet.Add(80);

            _session.Fail();

            Assert.That(_popups.Current, Is.SameAs(_outOfTime));
            Assert.That(_outOfTime.CanContinue, Is.True);
        }

        [Test]
        public void Fail_ShouldNotLetThePlayerContinue_WhenTheyCannotPay()
        {
            _wallet.Add(10);

            _session.Fail();

            Assert.That(_outOfTime.CanContinue, Is.False);
        }

        [Test]
        public void BuyTime_ShouldTakeTheCoinsCloseAndPlayOn_WhenTapped()
        {
            _wallet.Add(80);
            _session.Fail();
            int extra = 0;
            _bus.Subscribe<LevelContinuedSignal>(signal => extra = signal.ExtraSeconds);

            _buyTime.onClick.Invoke();

            Assert.That(_wallet.Coins, Is.EqualTo(30));
            Assert.That(extra, Is.EqualTo(_config.ContinueSeconds));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_popups.IsOpen, Is.False);
        }

        [Test]
        public void GiveUp_ShouldOpenLevelFailed_WhenTheOutOfTimePopupIsClosed()
        {
            _session.Fail();

            _giveUp.onClick.Invoke();

            Assert.That(_popups.Current, Is.SameAs(_levelFailed));
            Assert.That(_session.State, Is.EqualTo(LevelState.Failed));
        }

        [Test]
        public void Retry_ShouldCloseAndRestartTheLevel_WhenTapped()
        {
            _session.Fail();
            _giveUp.onClick.Invoke();

            _retry.onClick.Invoke();

            Assert.That(_popups.IsOpen, Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_session.LevelNumber, Is.EqualTo(1));
        }

        [Test]
        public void LevelFailedClose_ShouldGoHome_WhenTapped()
        {
            _session.Fail();
            _giveUp.onClick.Invoke();

            _goHome.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }
    }
}
