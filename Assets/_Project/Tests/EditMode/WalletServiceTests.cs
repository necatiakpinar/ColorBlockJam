using System;
using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The saved wallet (persistence.md).</summary>
    public sealed class WalletServiceTests
    {
        InMemoryKeyValueStorage _storage;
        RecordingLogger _logger;
        SignalBus _bus;
        readonly List<CoinsChangedSignal> _changes = new();

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemoryKeyValueStorage();
            _logger = new RecordingLogger();
            _bus = new SignalBus();
            _changes.Clear();
            _bus.Subscribe<CoinsChangedSignal>(_changes.Add);
        }

        WalletService CreateLoaded()
        {
            var wallet = new WalletService(_storage, _logger, _bus);
            wallet.Load();
            return wallet;
        }

        [Test]
        public void Wallet_ShouldStartEmpty_WhenNothingIsSaved()
        {
            Assert.That(CreateLoaded().Coins, Is.Zero);
        }

        [Test]
        public void Wallet_ShouldThrow_WhenReadBeforeLoad()
        {
            var wallet = new WalletService(_storage, _logger, _bus);

            Assert.Throws<InvalidOperationException>(() => _ = wallet.Coins);
        }

        [Test]
        public void Add_ShouldSaveAtOnceAndAnnounceOnce_WhenCoinsAreAdded()
        {
            WalletService wallet = CreateLoaded();

            wallet.Add(25);

            Assert.That(wallet.Coins, Is.EqualTo(25));
            Assert.That(_storage.FlushCount, Is.EqualTo(1));
            Assert.That(_changes.Count, Is.EqualTo(1));
            Assert.That(_changes[0].Coins, Is.EqualTo(25));
            Assert.That(_changes[0].Delta, Is.EqualTo(25));
            Assert.That(CreateLoaded().Coins, Is.EqualTo(25), "A fresh load reads the saved balance.");
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Add_ShouldRejectAndChangeNothing_WhenTheAmountIsNotPositive(int amount)
        {
            WalletService wallet = CreateLoaded();
            wallet.Add(10);

            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Add(amount));

            Assert.That(wallet.Coins, Is.EqualTo(10));
            Assert.That(_changes.Count, Is.EqualTo(1));
        }

        [Test]
        public void Load_ShouldStartEmptyAndLogError_WhenTheSaveIsCorrupt()
        {
            _storage.Set(WalletService.StorageKey, "{not json");

            Assert.That(CreateLoaded().Coins, Is.Zero);
            Assert.That(_logger.Errors, Has.Count.EqualTo(1));
        }

        [Test]
        public void Load_ShouldStartEmptyAndLogWarning_WhenTheSavedBalanceIsNegative()
        {
            _storage.Set(WalletService.StorageKey, "{\"_coins\":-3}");

            Assert.That(CreateLoaded().Coins, Is.Zero);
            Assert.That(_logger.Warnings, Has.Count.EqualTo(1));
        }
    }
}
