using System;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class PlayerProgressServiceTests
    {
        InMemoryKeyValueStorage _storage;
        RecordingLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemoryKeyValueStorage();
            _logger = new RecordingLogger();
        }

        PlayerProgressService CreateLoaded()
        {
            var service = new PlayerProgressService(_storage, _logger);
            service.Load();
            return service;
        }

        [Test]
        public void CurrentLevel_ShouldBeFirstLevel_WhenNothingIsStored()
        {
            Assert.That(CreateLoaded().CurrentLevel, Is.EqualTo(PlayerProgressData.FirstLevel));
        }

        [Test]
        public void CurrentLevel_ShouldThrow_WhenReadBeforeLoad()
        {
            var service = new PlayerProgressService(_storage, _logger);

            Assert.Throws<InvalidOperationException>(() => _ = service.CurrentLevel);
        }

        [Test]
        public void AdvanceLevel_ShouldPersistAndFlush_WhenCalled()
        {
            PlayerProgressService service = CreateLoaded();

            service.AdvanceLevel();

            Assert.That(service.CurrentLevel, Is.EqualTo(2));
            Assert.That(_storage.FlushCount, Is.EqualTo(1));
            Assert.That(CreateLoaded().CurrentLevel, Is.EqualTo(2), "a fresh load reads the saved level");
        }

        [Test]
        public void Load_ShouldStartFreshAndLogError_WhenStoredJsonIsCorrupt()
        {
            _storage.Set(PlayerProgressService.StorageKey, "{not json");

            PlayerProgressService service = CreateLoaded();

            Assert.That(service.CurrentLevel, Is.EqualTo(PlayerProgressData.FirstLevel));
            Assert.That(_logger.Errors, Has.Count.EqualTo(1));
        }

        [Test]
        public void Load_ShouldStartFreshAndLogWarning_WhenStoredLevelIsInvalid()
        {
            _storage.Set(PlayerProgressService.StorageKey, "{\"_currentLevel\":0}");

            PlayerProgressService service = CreateLoaded();

            Assert.That(service.CurrentLevel, Is.EqualTo(PlayerProgressData.FirstLevel));
            Assert.That(_logger.Warnings, Has.Count.EqualTo(1));
        }
    }
}
