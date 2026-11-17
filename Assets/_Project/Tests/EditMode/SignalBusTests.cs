using System;
using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class SignalBusTests
    {
        readonly struct TestSignal
        {
            public readonly int Value;

            public TestSignal(int value)
            {
                Value = value;
            }
        }

        readonly struct OtherSignal { }

        SignalBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new SignalBus();
        }

        [Test]
        public void Fire_ShouldDeliverPayload_WhenHandlerSubscribed()
        {
            int received = 0;
            _bus.Subscribe<TestSignal>(signal => received = signal.Value);

            _bus.Fire(new TestSignal(42));

            Assert.That(received, Is.EqualTo(42));
        }

        [Test]
        public void Fire_ShouldInvokeHandlersInSubscriptionOrder_WhenSeveralSubscribed()
        {
            var order = new List<string>();
            _bus.Subscribe<TestSignal>(_ => order.Add("first"));
            _bus.Subscribe<TestSignal>(_ => order.Add("second"));
            _bus.Subscribe<TestSignal>(_ => order.Add("third"));

            _bus.Fire(new TestSignal());

            Assert.That(order, Is.EqualTo(new[] { "first", "second", "third" }));
        }

        [Test]
        public void Fire_ShouldNotThrow_WhenNobodySubscribed()
        {
            Assert.DoesNotThrow(() => _bus.Fire(new TestSignal(1)));
        }

        [Test]
        public void Fire_ShouldOnlyReachMatchingSignalType_WhenTypesDiffer()
        {
            bool otherReceived = false;
            _bus.Subscribe<OtherSignal>(_ => otherReceived = true);

            _bus.Fire(new TestSignal());

            Assert.That(otherReceived, Is.False);
        }

        [Test]
        public void Unsubscribe_ShouldStopDelivery_WhenHandlerRemoved()
        {
            int calls = 0;
            void Handler(TestSignal _) => calls++;
            _bus.Subscribe<TestSignal>(Handler);

            _bus.Unsubscribe<TestSignal>(Handler);
            _bus.Fire(new TestSignal());

            Assert.That(calls, Is.Zero);
        }

        [Test]
        public void Unsubscribe_ShouldBeIgnored_WhenHandlerNeverSubscribed()
        {
            Assert.DoesNotThrow(() => _bus.Unsubscribe<TestSignal>(_ => { }));
        }

        [Test]
        public void Subscribe_ShouldThrow_WhenSameHandlerSubscribedTwice()
        {
            void Handler(TestSignal _) { }
            _bus.Subscribe<TestSignal>(Handler);

            Assert.Throws<InvalidOperationException>(() => _bus.Subscribe<TestSignal>(Handler));
        }

        [Test]
        public void Fire_ShouldStillReachLaterHandlers_WhenEarlierHandlerUnsubscribesItselfMidFire()
        {
            var order = new List<string>();
            Action<TestSignal> selfRemoving = null;
            selfRemoving = _ =>
            {
                order.Add("self-removing");
                _bus.Unsubscribe(selfRemoving);
            };
            _bus.Subscribe(selfRemoving);
            _bus.Subscribe<TestSignal>(_ => order.Add("later"));

            _bus.Fire(new TestSignal());
            _bus.Fire(new TestSignal());

            Assert.That(order, Is.EqualTo(new[] { "self-removing", "later", "later" }));
        }

        [Test]
        public void Fire_ShouldDeferNewHandlerToNextFire_WhenHandlerSubscribedMidFire()
        {
            int lateCalls = 0;
            bool added = false;
            _bus.Subscribe<TestSignal>(_ =>
            {
                if (added)
                    return;
                added = true;
                _bus.Subscribe<TestSignal>(_ => lateCalls++);
            });

            _bus.Fire(new TestSignal());
            Assert.That(lateCalls, Is.Zero);

            _bus.Fire(new TestSignal());
            Assert.That(lateCalls, Is.EqualTo(1));
        }
    }
}
