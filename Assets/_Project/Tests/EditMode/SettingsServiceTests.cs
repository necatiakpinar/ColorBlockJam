using System;
using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The saved toggles (persistence.md).</summary>
    public sealed class SettingsServiceTests
    {
        static readonly GameSetting[] AllSettings = { GameSetting.Music, GameSetting.Sound, GameSetting.Vibration };

        InMemoryKeyValueStorage _storage;
        RecordingLogger _logger;
        SignalBus _bus;
        readonly List<SettingChangedSignal> _changes = new();

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemoryKeyValueStorage();
            _logger = new RecordingLogger();
            _bus = new SignalBus();
            _changes.Clear();
            _bus.Subscribe<SettingChangedSignal>(_changes.Add);
        }

        SettingsService CreateLoaded()
        {
            var settings = new SettingsService(_storage, _logger, _bus);
            settings.Load();
            return settings;
        }

        [Test]
        public void Settings_ShouldAllBeOn_WhenNothingIsSaved()
        {
            SettingsService settings = CreateLoaded();

            foreach (GameSetting setting in AllSettings)
                Assert.That(settings.IsOn(setting), Is.True, setting.ToString());
        }

        [Test]
        public void Settings_ShouldThrow_WhenReadBeforeLoad()
        {
            var settings = new SettingsService(_storage, _logger, _bus);

            Assert.Throws<InvalidOperationException>(() => settings.IsOn(GameSetting.Music));
        }

        [Test]
        public void Set_ShouldSaveAndAnnounceOnlyThatToggle_WhenItChanges()
        {
            SettingsService settings = CreateLoaded();

            settings.Set(GameSetting.Sound, false);

            Assert.That(settings.IsOn(GameSetting.Sound), Is.False);
            Assert.That(settings.IsOn(GameSetting.Music), Is.True);
            Assert.That(settings.IsOn(GameSetting.Vibration), Is.True);
            Assert.That(_storage.FlushCount, Is.EqualTo(1));
            Assert.That(_changes.Count, Is.EqualTo(1));
            Assert.That(_changes[0].Setting, Is.EqualTo(GameSetting.Sound));
            Assert.That(_changes[0].IsOn, Is.False);
            Assert.That(CreateLoaded().IsOn(GameSetting.Sound), Is.False, "A fresh load reads the saved toggle.");
        }

        [Test]
        public void Set_ShouldDoNothing_WhenTheToggleAlreadyHasThatValue()
        {
            SettingsService settings = CreateLoaded();

            settings.Set(GameSetting.Music, true);

            Assert.That(_storage.FlushCount, Is.Zero);
            Assert.That(_changes, Is.Empty);
        }

        [Test]
        public void Load_ShouldStartWithEverythingOnAndLogError_WhenTheSaveIsCorrupt()
        {
            _storage.Set(SettingsService.StorageKey, "{not json");

            SettingsService settings = CreateLoaded();

            Assert.That(settings.IsOn(GameSetting.Vibration), Is.True);
            Assert.That(_logger.Errors, Has.Count.EqualTo(1));
        }
    }
}
