using System;
using NUnit.Framework;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The server time a probe brings, moved on by monotonic time (network.md, DA-27).</summary>
    public sealed class TrustedClockTests
    {
        FakeGameClock _clock;
        TrustedClock _trusted;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeGameClock();
            _trusted = new TrustedClock(_clock);
        }

        [Test]
        public void TrustedClock_ShouldHaveNoTime_BeforeAServerTimeArrives()
        {
            Assert.That(_trusted.TryGetUtcNow(out _), Is.False);
        }

        [Test]
        public void TrustedClock_ShouldReadTheHttpDateAsUtc_WhenRecorded()
        {
            bool recorded = _trusted.TryRecord("Tue, 17 Nov 2026 09:30:15 GMT");

            Assert.That(recorded, Is.True);
            Assert.That(_trusted.TryGetUtcNow(out DateTime now), Is.True);
            Assert.That(now, Is.EqualTo(new DateTime(2026, 11, 17, 9, 30, 15, DateTimeKind.Utc)));
            Assert.That(now.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void TrustedClock_ShouldMoveOnWithMonotonicTime_WhenTimePasses()
        {
            _trusted.TryRecord("Tue, 17 Nov 2026 09:30:15 GMT");

            _clock.Advance(90.5f);

            _trusted.TryGetUtcNow(out DateTime now);
            Assert.That(now, Is.EqualTo(new DateTime(2026, 11, 17, 9, 31, 45, 500, DateTimeKind.Utc)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("yesterday")]
        public void TrustedClock_ShouldKeepTheLastTime_WhenTheHeaderDoesNotParse(string header)
        {
            _trusted.TryRecord("Tue, 17 Nov 2026 09:30:15 GMT");

            bool recorded = _trusted.TryRecord(header);

            Assert.That(recorded, Is.False);
            _trusted.TryGetUtcNow(out DateTime now);
            Assert.That(now, Is.EqualTo(new DateTime(2026, 11, 17, 9, 30, 15, DateTimeKind.Utc)));
        }
    }
}
