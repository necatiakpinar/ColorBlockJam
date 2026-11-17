using System;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelRewardTests
    {
        const string ShippedConfigPath = "Assets/_Project/Configs/LevelRewardConfig.asset";

        TestCatalog _catalogs;
        LevelRewardConfig _config;
        LevelRewardProvider _provider;
        SignalBus _bus;
        WalletService _wallet;
        LevelRewardService _service;

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _config = ScriptableObject.CreateInstance<LevelRewardConfig>();
            _provider = new LevelRewardProvider(_config);
            _bus = new SignalBus();
            _wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _wallet.Load();
            var levels = new LevelProvider(_catalogs.Create(
                Level(LevelDifficulty.Normal), Level(LevelDifficulty.Hard), Level(LevelDifficulty.VeryHard)));
            _service = new LevelRewardService(_bus, levels, _provider, _wallet);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
            UnityEngine.Object.DestroyImmediate(_config);
            _catalogs.Dispose();
        }

        static LevelData Level(LevelDifficulty difficulty) =>
            new LevelDataBuilder(4, 4).Difficulty(difficulty).Block(Red, 1, 1).Door(Left, 1, 1, Red).Build();

        [TestCase(LevelDifficulty.Normal, 10)]
        [TestCase(LevelDifficulty.Hard, 25)]
        [TestCase(LevelDifficulty.VeryHard, 50)]
        public void Provider_ShouldPayByDifficulty_WhenAskedForALevel(LevelDifficulty difficulty, int coins)
        {
            Assert.That(_provider.CoinsFor(Level(difficulty)), Is.EqualTo(coins));
        }

        [Test]
        public void Provider_ShouldThrow_WhenTheConfigHasNoRewardForTheDifficulty()
        {
            Assert.Throws<InvalidOperationException>(() => _provider.CoinsFor((LevelDifficulty)999));
        }

        [Test]
        public void ShippedConfig_ShouldPayEveryDifficultyMoreThanTheOneBelow_WhenRead()
        {
            var shipped = new LevelRewardProvider(AssetDatabase.LoadAssetAtPath<LevelRewardConfig>(ShippedConfigPath));
            int previous = 0;
            foreach (LevelDifficulty difficulty in Enum.GetValues(typeof(LevelDifficulty)))
            {
                int coins = shipped.CoinsFor(difficulty);
                Assert.That(coins, Is.GreaterThan(previous), difficulty.ToString());
                previous = coins;
            }
        }

        [TestCase(1, 10)]
        [TestCase(2, 25)]
        [TestCase(3, 50)]
        public void Win_ShouldPayTheLevelsRewardIntoTheWallet_WhenTheLevelIsCompleted(int levelNumber, int coins)
        {
            _bus.Fire(new LevelCompletedSignal(levelNumber));

            Assert.That(_wallet.Coins, Is.EqualTo(coins));
        }

        [Test]
        public void Win_ShouldPayTheLoopedLayoutsReward_WhenTheCatalogHasLooped()
        {
            _bus.Fire(new LevelCompletedSignal(5));

            Assert.That(_wallet.Coins, Is.EqualTo(25), "Level 5 plays the second layout, a Hard one.");
        }

        [Test]
        public void Fail_ShouldNotTouchTheWallet_WhenTheLevelIsLost()
        {
            _bus.Fire(new LevelFailedSignal(1));

            Assert.That(_wallet.Coins, Is.Zero);
        }

        [Test]
        public void Win_ShouldNotPay_AfterTheServiceIsDisposed()
        {
            _service.Dispose();

            _bus.Fire(new LevelCompletedSignal(1));

            Assert.That(_wallet.Coins, Is.Zero);
        }
    }
}
