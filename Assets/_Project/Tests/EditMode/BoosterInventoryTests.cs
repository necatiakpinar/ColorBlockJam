using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class BoosterInventoryTests
    {
        InMemoryKeyValueStorage _storage;
        SignalBus _bus;
        BoosterConfig _config;
        readonly List<string> _events = new();

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemoryKeyValueStorage();
            _bus = new SignalBus();
            _config = ScriptableObject.CreateInstance<BoosterConfig>();
            _events.Clear();
            _bus.Subscribe<BoosterCountChangedSignal>(signal => _events.Add($"{signal.Type} {signal.Count}"));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        BoosterInventory Load()
        {
            var inventory = new BoosterInventory(_storage, new RecordingLogger(), _config, _bus);
            inventory.Load();
            return inventory;
        }

        [Test]
        public void NewPlayer_ShouldGetTheStartingStock_WhenNothingIsSaved()
        {
            BoosterInventory inventory = Load();

            foreach (BoosterSettings settings in _config.Boosters)
                Assert.That(inventory.Count(settings.Type), Is.EqualTo(settings.StartingCount), settings.Type.ToString());
            Assert.That(_storage.TryGet(BoosterInventory.StorageKey, out _), Is.True, "The starting stock is saved.");
        }

        [Test]
        public void TryUse_ShouldTakeOneAndSaveIt_WhenThePlayerHasOne()
        {
            BoosterInventory inventory = Load();

            bool used = inventory.TryUse(BoosterType.Hammer);

            Assert.That(used, Is.True);
            Assert.That(inventory.Count(BoosterType.Hammer), Is.Zero);
            Assert.That(_events, Is.EqualTo(new[] { "Hammer 0" }));
            Assert.That(Load().Count(BoosterType.Hammer), Is.Zero, "The use survives a reload.");
        }

        [Test]
        public void TryUse_ShouldChangeNothing_WhenNoneAreLeft()
        {
            BoosterInventory inventory = Load();
            inventory.TryUse(BoosterType.Rocket);
            _events.Clear();

            bool used = inventory.TryUse(BoosterType.Rocket);

            Assert.That(used, Is.False);
            Assert.That(inventory.Count(BoosterType.Rocket), Is.Zero);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void Add_ShouldThrow_WhenTheAmountIsNotPositive()
        {
            Assert.That(() => Load().Add(BoosterType.Freeze, 0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Load_ShouldStartFresh_WhenTheSaveHoldsANegativeCount()
        {
            _storage.Set(BoosterInventory.StorageKey, "{\"_stocks\":[{\"_type\":3000,\"_count\":-2}]}");

            Assert.That(Load().Count(BoosterType.Hammer), Is.EqualTo(_config.For(BoosterType.Hammer).StartingCount));
        }

        [Test]
        public void Shop_ShouldTakeThePriceAndAddOne_WhenThePlayerCanPay()
        {
            BoosterInventory inventory = Load();
            var wallet = new WalletService(_storage, new RecordingLogger(), _bus);
            wallet.Load();
            int price = _config.For(BoosterType.Vacuum).Price;
            wallet.Add(price + 5);
            var shop = new BoosterShop(wallet, inventory, _config, _storage);
            int flushes = _storage.FlushCount;

            bool bought = shop.TryBuy(BoosterType.Vacuum);

            Assert.That(bought, Is.True);
            Assert.That(wallet.Coins, Is.EqualTo(5));
            Assert.That(inventory.Count(BoosterType.Vacuum), Is.EqualTo(2));
            Assert.That(_storage.FlushCount - flushes, Is.EqualTo(1), "The coins and the booster are saved together.");
        }

        [Test]
        public void Shop_ShouldChangeNothing_WhenTheCoinsAreShort()
        {
            BoosterInventory inventory = Load();
            var wallet = new WalletService(_storage, new RecordingLogger(), _bus);
            wallet.Load();
            var shop = new BoosterShop(wallet, inventory, _config, _storage);

            bool bought = shop.TryBuy(BoosterType.Hammer);

            Assert.That(bought, Is.False);
            Assert.That(shop.CanAfford(BoosterType.Hammer), Is.False);
            Assert.That(inventory.Count(BoosterType.Hammer), Is.EqualTo(1));
        }

        [Test]
        public void ShippedConfig_ShouldGiveANewPlayerTwentyOfEachBooster_WhenTheGameShips()
        {
            var shipped = UnityEditor.AssetDatabase.LoadAssetAtPath<BoosterConfig>("Assets/_Project/Configs/BoosterConfig.asset");

            foreach (BoosterType type in System.Enum.GetValues(typeof(BoosterType)))
                Assert.That(shipped.For(type).StartingCount, Is.EqualTo(20), type.ToString());
        }
    }
}
