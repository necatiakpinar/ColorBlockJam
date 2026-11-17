using System;
using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Lives;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>Lives, their trusted-time refill and the coin refill (lives.md, DA-38).</summary>
    public sealed class LivesServiceTests
    {
        static readonly TimeSpan Refill = TimeSpan.FromMinutes(30);

        SignalBus _bus;
        LivesRig _rig;
        readonly List<string> _events = new();

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
            _rig = new LivesRig(_bus);
            _events.Clear();
            _bus.Subscribe<LivesChangedSignal>(signal => _events.Add($"{signal.Count} next {signal.SecondsToNextLife}"));
        }

        [TearDown]
        public void TearDown() => _rig.Dispose();

        LivesService Lives => _rig.Lives;

        [Test]
        public void NewPlayer_ShouldStartWithFullLives_WhenNothingIsSaved()
        {
            Assert.That(Lives.Count, Is.EqualTo(5));
            Assert.That(Lives.IsFull, Is.True);
            Assert.That(Lives.SecondsToNextLife(), Is.EqualTo(-1));
        }

        [Test]
        public void LoseOne_ShouldStartTheCountdown_WhenTheFirstLifeGoes()
        {
            bool lost = Lives.TryLoseOne();

            Assert.That(lost, Is.True);
            Assert.That(Lives.Count, Is.EqualTo(4));
            Assert.That(Lives.SecondsToNextLife(), Is.EqualTo(30 * 60));
            Assert.That(_events, Is.EqualTo(new[] { "4 next 1800" }));
        }

        [Test]
        public void Refresh_ShouldGiveALifeBack_WhenItsCountdownRunsOut()
        {
            Lives.TryLoseOne();
            Lives.TryLoseOne();
            _rig.Clock.Advance(Refill);

            Lives.Refresh();

            Assert.That(Lives.Count, Is.EqualTo(4));
            Assert.That(Lives.SecondsToNextLife(), Is.EqualTo(30 * 60), "The next life starts its own countdown.");
        }

        [Test]
        public void Refresh_ShouldCatchUpSeveralLivesButNeverPassTheMax_WhenLongTimePassed()
        {
            _rig.LoseAll();
            _rig.Clock.Advance(TimeSpan.FromHours(10));

            Lives.Refresh();

            Assert.That(Lives.Count, Is.EqualTo(5));
            Assert.That(Lives.SecondsToNextLife(), Is.EqualTo(-1));
        }

        [Test]
        public void Refresh_ShouldKeepTheRemainder_WhenTimeEndsBetweenTwoLives()
        {
            _rig.LoseAll();
            _rig.Clock.Advance(Refill + Refill + TimeSpan.FromMinutes(10));

            Lives.Refresh();

            Assert.That(Lives.Count, Is.EqualTo(2));
            Assert.That(Lives.SecondsToNextLife(), Is.EqualTo(20 * 60));
        }

        [Test]
        public void Countdown_ShouldWaitForTheTrustedTime_WhenALifeIsLostOffline()
        {
            _rig.Clock.Known = false;
            Lives.TryLoseOne();
            int unknown = Lives.SecondsToNextLife();

            _rig.Clock.Known = true;
            Lives.Refresh();

            Assert.That(unknown, Is.EqualTo(-1));
            Assert.That(Lives.SecondsToNextLife(), Is.EqualTo(30 * 60), "The countdown starts once the time is known.");
        }

        [Test]
        public void MovingTheDeviceClock_ShouldChangeNothing_BecauseOnlyTrustedTimeCounts()
        {
            Lives.TryLoseOne();

            // Only the trusted clock is read; nothing else can move the countdown.
            Lives.Refresh();

            Assert.That(Lives.Count, Is.EqualTo(4));
        }

        [Test]
        public void LoseOne_ShouldBeRefused_WhenNoLivesAreLeft()
        {
            _rig.LoseAll();

            Assert.That(Lives.TryLoseOne(), Is.False);
            Assert.That(Lives.Count, Is.Zero);
        }

        [Test]
        public void Refill_ShouldFillEveryLifeForCoins_WhenThePlayerCanPay()
        {
            _rig.LoseAll();
            _rig.Wallet.Add(Lives.RefillPrice + 3);

            bool refilled = Lives.TryRefill();

            Assert.That(refilled, Is.True);
            Assert.That(Lives.Count, Is.EqualTo(5));
            Assert.That(_rig.Wallet.Coins, Is.EqualTo(3));
            Assert.That(Lives.SecondsToNextLife(), Is.EqualTo(-1));
        }

        [Test]
        public void Refill_ShouldChangeNothing_WhenFullOrShortOfCoins()
        {
            _rig.Wallet.Add(Lives.RefillPrice);
            bool whenFull = Lives.TryRefill();
            Lives.TryLoseOne();
            _rig.Wallet.TrySpend(1);

            bool whenShort = Lives.TryRefill();

            Assert.That(whenFull, Is.False);
            Assert.That(whenShort, Is.False);
            Assert.That(Lives.Count, Is.EqualTo(4));
        }

        [Test]
        public void Lives_ShouldSurviveARestart_WhenSavedAndLoaded()
        {
            Lives.TryLoseOne();
            Lives.TryLoseOne();

            var reloaded = new LivesService(_rig.Storage, new RecordingLogger(), _rig.Config, _rig.Clock, _rig.Wallet, _bus);
            reloaded.Load();
            _rig.Clock.Advance(Refill);
            reloaded.Refresh();

            Assert.That(reloaded.Count, Is.EqualTo(4), "The countdown kept running while the app was closed.");
        }

        [Test]
        public void Clock_ShouldGiveLivesBackEverySecondTick_WhileOneIsMissing()
        {
            var clock = new LivesClock(Lives, new FakeGameClock());
            Lives.TryLoseOne();
            _events.Clear();
            _rig.Clock.Advance(TimeSpan.FromSeconds(1));

            clock.Tick();
            _rig.Clock.Advance(Refill);
            clock.Tick();
            clock.Tick();

            Assert.That(_events, Is.EqualTo(new[] { "4 next 1799", "5 next -1", "5 next -1" }));
            clock.Dispose();
        }
    }
}
