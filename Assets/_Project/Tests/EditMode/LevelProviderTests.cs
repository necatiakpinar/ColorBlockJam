using System;
using NUnit.Framework;
using RollicCase.Game.Levels;
using RollicCase.Tests.EditMode.Helpers;
using static RollicCase.Game.Levels.BlockColor;
using static RollicCase.Game.Levels.WallSide;

namespace RollicCase.Tests.EditMode
{
    public sealed class LevelProviderTests
    {
        TestCatalog _catalogs;

        [SetUp]
        public void SetUp() => _catalogs = new TestCatalog();

        [TearDown]
        public void TearDown() => _catalogs.Dispose();

        static LevelData LevelOfWidth(int width) =>
            new LevelDataBuilder(width, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build();

        [TestCase(1, 3)]
        [TestCase(2, 4)]
        [TestCase(3, 5)]
        public void GetLevel_ShouldReturnCatalogEntry_WhenNumberIsInRange(int levelNumber, int expectedWidth)
        {
            var provider = new LevelProvider(_catalogs.Create(LevelOfWidth(3), LevelOfWidth(4), LevelOfWidth(5)));

            Assert.That(provider.GetLevel(levelNumber).Width, Is.EqualTo(expectedWidth));
        }

        [TestCase(4, 3)]
        [TestCase(8, 4)]
        public void GetLevel_ShouldLoopTheCatalog_WhenNumberIsPastTheLastLevel(int levelNumber, int expectedWidth)
        {
            var provider = new LevelProvider(_catalogs.Create(LevelOfWidth(3), LevelOfWidth(4), LevelOfWidth(5)));

            Assert.That(provider.GetLevel(levelNumber).Width, Is.EqualTo(expectedWidth));
        }

        [Test]
        public void GetLevel_ShouldThrow_WhenCatalogIsEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => new LevelProvider(_catalogs.Create()).GetLevel(1));
        }

        [Test]
        public void GetLevel_ShouldThrow_WhenNumberIsBelowOne()
        {
            var provider = new LevelProvider(_catalogs.Create(LevelOfWidth(3)));

            Assert.Throws<ArgumentOutOfRangeException>(() => provider.GetLevel(0));
        }
    }
}
