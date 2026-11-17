using System;
using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
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

        LevelRewardConfig _config;
        LevelRewardProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LevelRewardConfig>();
            _provider = new LevelRewardProvider(_config);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_config);
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
    }
}
