using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class ConnectivityServiceTests
    {
        const float Online = 10f;
        const float Offline = 3f;
        const string ServerDate = "Tue, 17 Nov 2026 09:30:15 GMT";

        FakeProbeRequest _probe;
        FakeGameClock _clock;
        TrustedClock _trusted;
        SignalBus _bus;
        ConnectivityService _service;
        readonly List<bool> _changes = new();

        [SetUp]
        public void SetUp()
        {
            _probe = new FakeProbeRequest();
            _clock = new FakeGameClock();
            _trusted = new TrustedClock(_clock);
            _bus = new SignalBus();
            _service = new ConnectivityService(_probe, _clock, _trusted, _bus, new RecordingLogger(),
                new ConnectivitySettings("https://probe.test", 5f, Online, Offline));
            _changes.Clear();
            _bus.Subscribe<ConnectivityChangedSignal>(signal => _changes.Add(signal.IsOnline));
        }

        [TearDown]
        public void TearDown() => _service.Dispose();

        [Test]
        public void Service_ShouldNotKnow_BeforeTheFirstProbeAnswers()
        {
            _service.Start();

            Assert.That(_probe.Sent, Is.EqualTo(1));
            Assert.That(_service.HasProbed, Is.False);
            Assert.That(_service.IsOnline, Is.False);
            Assert.That(_changes, Is.Empty);
        }

        [Test]
        public void Service_ShouldAnnounceTheFirstAnswer_EvenWhenItIsOffline()
        {
            _service.Start();

            _probe.Answer(false);

            Assert.That(_service.HasProbed, Is.True);
            Assert.That(_changes, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void Service_ShouldAnnounceOnlyChanges_WhenProbesRepeat()
        {
            _service.Start();
            _probe.Answer(true);
            _clock.Advance(Online);
            _probe.Answer(true);
            _clock.Advance(Online);
            _probe.Answer(false);
            _clock.Advance(Offline);
            _probe.Answer(false);
            _clock.Advance(Offline);

            _probe.Answer(true);

            Assert.That(_changes, Is.EqualTo(new[] { true, false, true }));
            Assert.That(_service.IsOnline, Is.True);
        }

        [Test]
        public void Service_ShouldProbeAtTheOnlineInterval_WhileOnline()
        {
            _service.Start();
            _probe.Answer(true);

            _clock.Advance(Online - 0.1f);
            int beforeInterval = _probe.Sent;
            _clock.Advance(0.1f);

            Assert.That(beforeInterval, Is.EqualTo(1));
            Assert.That(_probe.Sent, Is.EqualTo(2));
        }

        [Test]
        public void Service_ShouldProbeSooner_WhileOffline()
        {
            _service.Start();
            _probe.Answer(false);

            _clock.Advance(Offline);

            Assert.That(_probe.Sent, Is.EqualTo(2));
        }

        [Test]
        public void Service_ShouldProbeAtOnce_WhenTheAppComesBack()
        {
            _service.Start();
            _probe.Answer(true);

            _bus.Fire(new ApplicationPausedSignal(true));
            int whileAway = _probe.Sent;
            _bus.Fire(new ApplicationPausedSignal(false));

            Assert.That(whileAway, Is.EqualTo(1));
            Assert.That(_probe.Sent, Is.EqualTo(2));
        }

        [Test]
        public void ProbeNow_ShouldProbeAtOnceAndReturnTheAnswer_WhenRetried()
        {
            _service.Start();
            _probe.Answer(false);

            UniTask<bool> retry = _service.ProbeNowAsync();
            int sent = _probe.Sent;
            _probe.Answer(true);

            Assert.That(sent, Is.EqualTo(2), "Did not wait for the offline interval.");
            Assert.That(retry.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(retry.GetAwaiter().GetResult(), Is.True);
        }

        [Test]
        public void Service_ShouldGiveTheTrustedClockTheServerTime_WhenAProbeSucceeds()
        {
            _service.Start();

            _probe.Answer(true, ServerDate);
            _clock.Advance(2f);

            Assert.That(_trusted.TryGetUtcNow(out DateTime now), Is.True);
            Assert.That(now, Is.EqualTo(new DateTime(2026, 11, 17, 9, 30, 17, DateTimeKind.Utc)));
        }

        [Test]
        public void Service_ShouldStopProbing_WhenDisposed()
        {
            _service.Start();
            _probe.Answer(true);

            _service.Dispose();
            _clock.Advance(Online * 3f);

            Assert.That(_probe.Sent, Is.EqualTo(1));
        }
    }
}
