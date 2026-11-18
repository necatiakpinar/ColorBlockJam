using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ColorBlockJam.Core.Assets;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Audio;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using SettingsService = ColorBlockJam.Core.Persistence.SettingsService;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class AudioServiceTests
    {
        const string ShippedConfigPath = "Assets/_Project/Configs/AudioConfig.asset";
        const string TapClipGuid = "0123456789abcdef0123456789abcdef";
        const string WinClipGuid = "abcdef0123456789abcdef0123456789";

        AudioConfig _config;
        AudioClip _tapClip;
        AudioClip _winClip;
        FakeAssetSource _source;
        AssetLoader _assets;
        SettingsService _settings;
        RecordingAudioOutput _output;
        AudioService _audio;

        [SetUp]
        public void SetUp()
        {
            _tapClip = AudioClip.Create("tap", 10, 1, 22050, false);
            _winClip = AudioClip.Create("win", 10, 1, 22050, false);
            _config = ScriptableObject.CreateInstance<AudioConfig>();
            var serialized = new SerializedObject(_config);
            SerializedProperty cues = serialized.FindProperty("_cues");
            cues.arraySize = 2;
            SetCue(cues.GetArrayElementAtIndex(0), AudioCue.Tap, TapClipGuid, 0.4f);
            SetCue(cues.GetArrayElementAtIndex(1), AudioCue.Win, WinClipGuid, 0.9f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _settings = new SettingsService(new InMemoryKeyValueStorage(), new RecordingLogger(), new SignalBus());
            _settings.Load();
            _output = new RecordingAudioOutput();
            _source = new FakeAssetSource();
            _source.Add(TapClipGuid, _tapClip);
            _source.Add(WinClipGuid, _winClip);
            _assets = new AssetLoader(_source);
            _audio = new AudioService(_config, _settings, _output, _assets);
        }

        static void SetCue(SerializedProperty entry, AudioCue cue, string clipGuid, float volume)
        {
            entry.FindPropertyRelative("_cue").intValue = (int)cue;
            entry.FindPropertyRelative("_clip.m_AssetGUID").stringValue = clipGuid;
            entry.FindPropertyRelative("_volume").floatValue = volume;
        }

        void LoadSounds() => _audio.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();

        [TearDown]
        public void TearDown()
        {
            _assets.Dispose();
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_tapClip);
            Object.DestroyImmediate(_winClip);
        }

        [Test]
        public void Play_ShouldPlayTheCuesClipAtItsVolume_WhenSoundIsOn()
        {
            LoadSounds();

            _audio.Play(AudioCue.Tap);

            Assert.That(_output.Clips, Is.EqualTo(new[] { _tapClip }));
            Assert.That(_output.Volumes, Is.EqualTo(new[] { 0.4f }));
        }

        [Test]
        public void Play_ShouldPlayEachCuesOwnClip_WhenSeveralAreLoaded()
        {
            LoadSounds();

            _audio.Play(AudioCue.Win);
            _audio.Play(AudioCue.Tap);

            Assert.That(_output.Clips, Is.EqualTo(new[] { _winClip, _tapClip }));
            Assert.That(_output.Volumes, Is.EqualTo(new[] { 0.9f, 0.4f }));
            Assert.That(_source.Loads, Is.EquivalentTo(new[] { TapClipGuid, WinClipGuid }));
        }

        [Test]
        public void Play_ShouldStaySilent_WhenSoundIsOff()
        {
            LoadSounds();
            _settings.Set(GameSetting.Sound, false);

            _audio.Play(AudioCue.Tap);

            Assert.That(_output.Clips, Is.Empty);
        }

        [Test]
        public void Play_ShouldThrow_WhenTheCueHasNoClip()
        {
            LoadSounds();

            Assert.Throws<InvalidOperationException>(() => _audio.Play(AudioCue.Exit));
        }

        [Test]
        public void Play_ShouldThrow_WhenTheSoundsAreNotLoaded()
        {
            Assert.Throws<InvalidOperationException>(() => _audio.Play(AudioCue.Tap));
        }

        [Test]
        public void Play_ShouldThrow_WhenTheLoadFailed()
        {
            _source.Hold(WinClipGuid).TrySetException(new KeyNotFoundException(WinClipGuid));

            Assert.Throws<KeyNotFoundException>(LoadSounds);
            Assert.Throws<InvalidOperationException>(() => _audio.Play(AudioCue.Tap));
        }

        [Test]
        public void ShippedConfig_ShouldHoldAClipForEveryCue_WhenTheGameShips()
        {
            var shipped = AssetDatabase.LoadAssetAtPath<AudioConfig>(ShippedConfigPath);

            foreach (AudioCue cue in Enum.GetValues(typeof(AudioCue)))
                Assert.That(shipped.For(cue).Clip.editorAsset, Is.Not.Null, cue.ToString());
        }
    }
}
