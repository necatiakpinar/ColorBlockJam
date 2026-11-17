using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class GameplayHudViewTests
    {
        TestViews _views;
        SignalBus _bus;
        GameplayHudView _hud;
        WalletService _wallet;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _bus = new SignalBus();
            _wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _wallet.Load();
            _hud = _views.CreateHud(_bus, _wallet, out _, out _);
        }

        [TearDown]
        public void TearDown() => _views.Dispose();

        [TestCase(150, "02:30")]
        [TestCase(59, "00:59")]
        [TestCase(0, "00:00")]
        [TestCase(600, "10:00")]
        public void Timer_ShouldShowMinutesAndSeconds_WhenTheTimerChanges(int secondsLeft, string expected)
        {
            _bus.Fire(new TimerChangedSignal(secondsLeft));

            Assert.That(_hud.TimerLabelText, Is.EqualTo(expected));
        }

        [Test]
        public void Level_ShouldShowTheLevelNumberAndTheBalance_WhenALevelStarts()
        {
            _wallet.Add(320);

            _bus.Fire(new LevelStartedSignal(23));

            Assert.That(_hud.LevelLabelText, Is.EqualTo("23"));
            Assert.That(_hud.CoinLabelText, Is.EqualTo("320"));
        }

        [TestCase(999, "999")]
        [TestCase(1280, "1.28k")]
        public void Coins_ShouldFollowTheWallet_WhenTheBalanceChanges(int coins, string expected)
        {
            _wallet.Add(coins);

            Assert.That(_hud.CoinLabelText, Is.EqualTo(expected));
        }

        [Test]
        public void Coins_ShouldWaitForTheFlyingCoins_WhenHeldAfterAWin()
        {
            _wallet.Add(100);
            _hud.HoldCoins(100);
            _wallet.Add(25);
            string whilePaid = _hud.CoinLabelText;

            _hud.AddHeldCoins(10);
            string afterOneCoin = _hud.CoinLabelText;
            _hud.AddHeldCoins(15);

            Assert.That(whilePaid, Is.EqualTo("100"), "The paid reward does not jump onto the counter.");
            Assert.That(afterOneCoin, Is.EqualTo("110"));
            Assert.That(_hud.CoinLabelText, Is.EqualTo("125"));
        }

        [Test]
        public void Coins_ShouldFollowTheWalletAgain_WhenTheNextLevelStarts()
        {
            _hud.HoldCoins(0);

            _bus.Fire(new LevelStartedSignal(2));
            _wallet.Add(40);

            Assert.That(_hud.CoinLabelText, Is.EqualTo("40"));
        }
    }
}
