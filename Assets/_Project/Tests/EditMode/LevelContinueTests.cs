using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelContinueTests
    {
        const int TimerSeconds = 12;

        TestCatalog _catalogs;
        SignalBus _bus;
        LevelSession _session;
        WalletService _wallet;
        GameplayConfig _config;
        FakeGameClock _clock;
        LevelTimer _timer;
        LevelContinueService _continues;

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            var levels = new LevelProvider(_catalogs.Create(new LevelDataBuilder(4, 4).Timer(TimerSeconds)
                .Block(Red, 1, 1).Block(Blue, 3, 3).Door(Left, 1, 1, Red).Door(Right, 3, 1, Blue).Build()));
            _session = new LevelSession(new ActiveBoard(new BoardSpace(1f), _bus), levels, _bus);
            _wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _wallet.Load();
            _config = ScriptableObject.CreateInstance<GameplayConfig>();
            _clock = new FakeGameClock();
            _timer = new LevelTimer(_session, _clock, _bus);
            _continues = new LevelContinueService(_session, _wallet, _config);
            _session.Start(1);
            _bus.Fire(new BlockGrabbedSignal(0));
        }

        [TearDown]
        public void TearDown()
        {
            _timer.Dispose();
            _session.Dispose();
            Object.DestroyImmediate(_config);
            _catalogs.Dispose();
        }

        [Test]
        public void TrySpend_ShouldTakeTheCoins_WhenThePlayerHasThem()
        {
            _wallet.Add(60);

            Assert.That(_wallet.TrySpend(50), Is.True);
            Assert.That(_wallet.Coins, Is.EqualTo(10));
        }

        [Test]
        public void TrySpend_ShouldChangeNothing_WhenThePlayerHasTooFew()
        {
            _wallet.Add(40);

            Assert.That(_wallet.TrySpend(50), Is.False);
            Assert.That(_wallet.Coins, Is.EqualTo(40));
        }

        [Test]
        public void Continue_ShouldPayAndGiveTheSecondsAndCountAtOnce_WhenTheLevelRanOutOfTime()
        {
            _wallet.Add(60);
            _clock.Advance(TimerSeconds);

            bool continued = _continues.TryContinue();
            _clock.Advance(1f);

            Assert.That(continued, Is.True);
            Assert.That(_wallet.Coins, Is.EqualTo(10));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_timer.SecondsLeft, Is.EqualTo(_config.ContinueSeconds - 1f).Within(1e-4f));
        }

        [Test]
        public void Continue_ShouldFailTheLevelAgain_WhenTheBoughtTimeRunsOut()
        {
            _wallet.Add(60);
            _clock.Advance(TimerSeconds);
            _continues.TryContinue();
            int fails = 0;
            _bus.Subscribe<LevelFailedSignal>(_ => fails++);

            _clock.Advance(_config.ContinueSeconds);

            Assert.That(fails, Is.EqualTo(1));
            Assert.That(_session.State, Is.EqualTo(LevelState.Failed));
        }

        [Test]
        public void Continue_ShouldChangeNothing_WhenThePlayerCannotPay()
        {
            _wallet.Add(10);
            _clock.Advance(TimerSeconds);

            Assert.That(_continues.CanAfford, Is.False);
            Assert.That(_continues.TryContinue(), Is.False);
            Assert.That(_wallet.Coins, Is.EqualTo(10));
            Assert.That(_session.State, Is.EqualTo(LevelState.Failed));
        }

        [Test]
        public void Continue_ShouldNotCharge_WhenTheLevelIsNotLost()
        {
            _wallet.Add(60);

            Assert.That(_continues.TryContinue(), Is.False);
            Assert.That(_wallet.Coins, Is.EqualTo(60));
        }

        [Test]
        public void SessionContinue_ShouldBeRefused_WhenTheLevelIsPlaying()
        {
            Assert.That(_session.Continue(20), Is.False);
        }
    }
}
