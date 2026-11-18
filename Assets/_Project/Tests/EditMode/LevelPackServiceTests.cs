using System;
using System.Threading;
using NUnit.Framework;
using ColorBlockJam.Core.Assets;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Packs;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelPackServiceTests
    {
        const int BundledRevision = 5;
        const string BundledGuid = "fedcba9876543210fedcba9876543210";

        readonly LevelPackJson _json = new();
        InMemoryKeyValueStorage _storage;
        RecordingLogger _logger;
        LevelRulesConfig _rules;
        LevelPackConfig _config;
        TextAsset _bundled;
        FakeAssetSource _source;

        static LevelData Level(int width) => new LevelDataBuilder(width, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build();

        string Pack(int revision, params int[] widths)
        {
            var levels = new LevelData[widths.Length];
            for (int i = 0; i < widths.Length; i++)
                levels[i] = Level(widths[i]);
            return _json.Write(new LevelPack(revision, levels));
        }

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemoryKeyValueStorage();
            _logger = new RecordingLogger();
            _rules = ScriptableObject.CreateInstance<LevelRulesConfig>();
            _source = new FakeAssetSource();
            UseBundled(Pack(BundledRevision, 4, 5));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_bundled);
            Object.DestroyImmediate(_rules);
        }

        void UseBundled(string json)
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_bundled);
            _bundled = new TextAsset(json);
            _config = ScriptableObject.CreateInstance<LevelPackConfig>();
            var serialized = new SerializedObject(_config);
            serialized.FindProperty("_bundledPack.m_AssetGUID").stringValue = BundledGuid;
            _source.Add(BundledGuid, _bundled);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        LevelPackService Loaded()
        {
            LevelPackService service = Unloaded();
            service.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
            return service;
        }

        LevelPackService Unloaded() => new(_config, _rules, _storage, _logger, new AssetLoader(_source));

        [Test]
        public void LoadAsync_ShouldReleaseTheBundledPack_WhenItHasBeenRead()
        {
            Loaded();

            Assert.That(_source.Loads, Is.EqualTo(new[] { BundledGuid }));
            Assert.That(_source.Releases, Is.EqualTo(new[] { BundledGuid }));
        }

        [Test]
        public void LoadAsync_ShouldPlayTheBundledPack_WhenNothingIsStored()
        {
            LevelPackService service = Loaded();

            Assert.That(service.IsRemote, Is.False);
            Assert.That(service.LevelCount, Is.EqualTo(2));
            Assert.That(service.GetLevel(1).Width, Is.EqualTo(5));
        }

        [Test]
        public void LoadAsync_ShouldPlayTheStoredRemotePack_WhenItIsValidAndNotOlder()
        {
            _storage.Set(LevelPackService.RemoteKey, Pack(BundledRevision, 6, 7, 8));

            LevelPackService service = Loaded();

            Assert.That(service.IsRemote, Is.True);
            Assert.That(service.LevelCount, Is.EqualTo(3));
            Assert.That(service.GetLevel(0).Width, Is.EqualTo(6));
        }

        [Test]
        public void LoadAsync_ShouldDropTheStoredPack_WhenTheBuildShipsANewerOne()
        {
            _storage.Set(LevelPackService.RemoteKey, Pack(BundledRevision - 1, 6));

            LevelPackService service = Loaded();

            Assert.That(service.IsRemote, Is.False);
            Assert.That(_storage.TryGet(LevelPackService.RemoteKey, out _), Is.False);
            Assert.That(_logger.Warnings, Has.Count.EqualTo(1));
        }

        [Test]
        public void LoadAsync_ShouldDropTheStoredPack_WhenItHasAnUnplayableLevel()
        {
            _storage.Set(LevelPackService.RemoteKey, Pack(BundledRevision + 1, 2));

            LevelPackService service = Loaded();

            Assert.That(service.IsRemote, Is.False);
            Assert.That(_storage.TryGet(LevelPackService.RemoteKey, out _), Is.False);
            Assert.That(_logger.Warnings[0], Does.Contain("Level 1"));
        }

        [Test]
        public void TryStoreRemote_ShouldKeepThePackForTheNextLaunch_WhenItIsValid()
        {
            LevelPackService service = Loaded();

            Assert.That(service.TryStoreRemote(Pack(BundledRevision + 1, 9), out string error), Is.True, error);

            Assert.That(service.LevelCount, Is.EqualTo(2), "This launch keeps its levels.");
            Assert.That(Loaded().GetLevel(0).Width, Is.EqualTo(9), "The next launch plays the new pack.");
        }

        [TestCase(BundledRevision - 1, 6, "older")]
        [TestCase(BundledRevision + 1, 2, "Level 1")]
        public void TryStoreRemote_ShouldStoreNothing_WhenThePackIsOlderOrBroken(int revision, int width, string reason)
        {
            LevelPackService service = Loaded();

            Assert.That(service.TryStoreRemote(Pack(revision, width), out string error), Is.False);

            Assert.That(error, Does.Contain(reason));
            Assert.That(_storage.TryGet(LevelPackService.RemoteKey, out _), Is.False);
        }

        [Test]
        public void TryStoreRemote_ShouldStoreNothing_WhenTheJsonDoesNotRead()
        {
            Assert.That(Loaded().TryStoreRemote("{ broken", out string error), Is.False);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void Levels_ShouldThrow_WhenReadBeforeLoad()
        {
            LevelPackService service = Unloaded();

            Assert.Throws<InvalidOperationException>(() => _ = service.LevelCount);
            Assert.Throws<InvalidOperationException>(() => service.TryStoreRemote(Pack(BundledRevision + 1, 9), out _));
        }

        [Test]
        public void LoadAsync_ShouldThrow_WhenTheBundledPackIsBroken()
        {
            UseBundled("{}");

            Assert.Throws<InvalidOperationException>(() => Loaded());
        }
    }
}
