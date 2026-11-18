using NUnit.Framework;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.UI;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine.UI;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelAttemptLivesTests
    {
        TestViews _views;
        TestCatalog _catalogs;
        SignalBus _bus;
        LevelSession _session;
        LivesRig _lives;
        LevelAttemptLives _attempt;
        RecordingPopupService _popups;
        RecordingNavigation _navigation;
        OutOfLivesPopupView _popup;
        Button _refill;
        Button _close;
        GameplayLivesFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            var levels = new LevelProvider(_catalogs.Create(
                new LevelDataBuilder(4, 4).Block(Red, 0, 0).Block(Blue, 3, 3).Door(Left, 0, 1, Red).Door(Right, 3, 1, Blue).Build()));
            _session = new LevelSession(new ActiveBoard(new BoardSpace(1f), _bus), levels, _bus);
            _lives = new LivesRig(_bus);
            _attempt = new LevelAttemptLives(_lives.Lives, _bus);
            _popups = new RecordingPopupService();
            _navigation = new RecordingNavigation();
            _popup = _views.CreateOutOfLivesPopup(_bus, out _refill, out _close);
            _flow = new GameplayLivesFlow(_lives.Lives, _lives.Wallet, _popup, _popups, _session, _navigation);
            _session.Start(1);
        }

        [TearDown]
        public void TearDown()
        {
            _flow.Dispose();
            _attempt.Dispose();
            _session.Dispose();
            _lives.Dispose();
            _views.Dispose();
            _catalogs.Dispose();
        }

        void Grab() => _bus.Fire(new BlockGrabbedSignal(0));

        [Test]
        public void Abandon_ShouldCostNothing_WhenThePlayerNeverTouchedTheLevel()
        {
            _attempt.Abandon();

            Assert.That(_lives.Lives.Count, Is.EqualTo(5));
        }

        [Test]
        public void Abandon_ShouldCostOneLifeOnce_WhenAStartedAttemptIsLeft()
        {
            Grab();

            _attempt.Abandon();
            _attempt.Abandon();

            Assert.That(_lives.Lives.Count, Is.EqualTo(4));
        }

        [Test]
        public void Abandon_ShouldCostNothing_WhenTheLevelWasWon()
        {
            Grab();
            _bus.Fire(new BoardClearedSignal());

            _attempt.Abandon();

            Assert.That(_lives.Lives.Count, Is.EqualTo(5));
        }

        [Test]
        public void Fail_ShouldCostOneLifeOnce_WhenAStartedAttemptRunsOutOfTime()
        {
            Grab();

            _session.Fail();
            _attempt.Abandon();

            Assert.That(_lives.Lives.Count, Is.EqualTo(4));
            Assert.That(_attempt.WouldCostLife, Is.False);
        }

        [Test]
        public void Fail_ShouldCostNothing_WhenThePlayerNeverTouchedTheLevel()
        {
            _session.Fail();

            Assert.That(_lives.Lives.Count, Is.EqualTo(5));
        }

        [Test]
        public void NewAttempt_ShouldCostItsOwnLife_WhenItIsStartedAndLeftToo()
        {
            Grab();
            _attempt.Abandon();
            _session.Restart();
            Grab();

            _attempt.Abandon();

            Assert.That(_lives.Lives.Count, Is.EqualTo(3));
        }

        [Test]
        public void PlayAgain_ShouldRestartAtOnce_WhenALifeIsLeft()
        {
            Grab();

            _flow.PlayAgain();

            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_popups.Current, Is.Null);
        }

        [Test]
        public void PlayAgain_ShouldPauseAndOfferARefill_WhenNoLivesAreLeft()
        {
            _lives.LoseAll();

            _flow.PlayAgain();

            Assert.That(_session.State, Is.EqualTo(LevelState.Paused));
            Assert.That(_popups.Current, Is.SameAs(_popup));
        }

        [Test]
        public void PlayAgain_ShouldDoNothing_WhenTheLevelWasWon()
        {
            Grab();
            _bus.Fire(new BoardClearedSignal());
            _lives.LoseAll();

            _flow.PlayAgain();

            Assert.That(_session.State, Is.EqualTo(LevelState.Won));
            Assert.That(_popups.Current, Is.Null, "The win popup is on its way; nothing may open over it.");
        }

        [Test]
        public void Refill_ShouldFillTheLivesAndRestart_WhenBoughtInGameplay()
        {
            _lives.LoseAll();
            _lives.Wallet.Add(_lives.Lives.RefillPrice);
            _flow.PlayAgain();

            _refill.onClick.Invoke();

            Assert.That(_lives.Lives.Count, Is.EqualTo(5));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_popups.Current, Is.Null);
        }

        [Test]
        public void Close_ShouldGoHome_WhenThePlayerDoesNotRefill()
        {
            _lives.LoseAll();
            _flow.PlayAgain();

            _close.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Home }));
        }
    }
}
