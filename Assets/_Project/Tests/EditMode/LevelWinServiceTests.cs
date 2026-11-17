using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelWinServiceTests
    {
        TestCatalog _catalogs;
        LevelRewardConfig _config;
        SignalBus _bus;
        InMemoryKeyValueStorage _storage;
        PlayerProgressService _progress;
        WalletService _wallet;
        LevelWinService _service;

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _config = ScriptableObject.CreateInstance<LevelRewardConfig>();
            _bus = new SignalBus();
            _storage = new InMemoryKeyValueStorage();
            _progress = new PlayerProgressService(_storage, new RecordingLogger());
            _progress.Load();
            _wallet = new WalletService(_storage, new RecordingLogger(), _bus);
            _wallet.Load();
            var levels = new LevelProvider(_catalogs.Create(
                Level(LevelDifficulty.Normal), Level(LevelDifficulty.Hard), Level(LevelDifficulty.VeryHard)));
            _service = new LevelWinService(_bus, levels, new LevelRewardProvider(_config), _progress, _wallet, _storage);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
            Object.DestroyImmediate(_config);
            _catalogs.Dispose();
        }

        static LevelData Level(LevelDifficulty difficulty) =>
            new LevelDataBuilder(4, 4).Difficulty(difficulty).Block(Red, 1, 1).Door(Left, 1, 1, Red).Build();

        [TestCase(1, 10)]
        [TestCase(2, 25)]
        [TestCase(3, 50)]
        public void Win_ShouldPayTheLevelsRewardAndAdvanceOnce_WhenTheLevelIsCompleted(int levelNumber, int coins)
        {
            _bus.Fire(new LevelCompletedSignal(levelNumber));

            Assert.That(_wallet.Coins, Is.EqualTo(coins));
            Assert.That(_progress.CurrentLevel, Is.EqualTo(2));
        }

        [Test]
        public void Win_ShouldPayTheLoopedLayoutsReward_WhenTheCatalogHasLooped()
        {
            _bus.Fire(new LevelCompletedSignal(5));

            Assert.That(_wallet.Coins, Is.EqualTo(25), "Level 5 plays the second layout, a Hard one.");
        }

        [Test]
        public void Win_ShouldSaveTheProgressAndTheCoinsInOneFlush_WhenTheLevelIsCompleted()
        {
            int flushes = _storage.FlushCount;

            _bus.Fire(new LevelCompletedSignal(1));

            Assert.That(_storage.FlushCount - flushes, Is.EqualTo(1));
        }

        [Test]
        public void Win_ShouldAnnounceThePaidRewardAndTheBalanceBefore_WhenTheLevelIsCompleted()
        {
            _wallet.Add(7);
            LevelRewardedSignal rewarded = default;
            _bus.Subscribe<LevelRewardedSignal>(signal => rewarded = signal);

            _bus.Fire(new LevelCompletedSignal(2));

            Assert.That(rewarded.LevelNumber, Is.EqualTo(2));
            Assert.That(rewarded.Coins, Is.EqualTo(25));
            Assert.That(rewarded.BalanceBefore, Is.EqualTo(7));
        }

        [Test]
        public void Fail_ShouldNotTouchTheWalletOrTheProgress_WhenTheLevelIsLost()
        {
            _bus.Fire(new LevelFailedSignal(1));

            Assert.That(_wallet.Coins, Is.Zero);
            Assert.That(_progress.CurrentLevel, Is.EqualTo(1));
        }

        [Test]
        public void Win_ShouldChangeNothing_AfterTheServiceIsDisposed()
        {
            _service.Dispose();

            _bus.Fire(new LevelCompletedSignal(1));

            Assert.That(_wallet.Coins, Is.Zero);
            Assert.That(_progress.CurrentLevel, Is.EqualTo(1));
        }
    }
}
