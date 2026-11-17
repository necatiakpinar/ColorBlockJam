using System;
using NUnit.Framework;
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

        AudioConfig _config;
        AudioClip _tapClip;
        SettingsService _settings;
        RecordingAudioOutput _output;
        AudioService _audio;

        [SetUp]
        public void SetUp()
        {
            _tapClip = AudioClip.Create("tap", 10, 1, 22050, false);
            _config = ScriptableObject.CreateInstance<AudioConfig>();
            var serialized = new SerializedObject(_config);
            SerializedProperty cues = serialized.FindProperty("_cues");
            cues.arraySize = 1;
            SerializedProperty entry = cues.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("_cue").intValue = (int)AudioCue.Tap;
            entry.FindPropertyRelative("_clip").objectReferenceValue = _tapClip;
            entry.FindPropertyRelative("_volume").floatValue = 0.4f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _settings = new SettingsService(new InMemoryKeyValueStorage(), new RecordingLogger(), new SignalBus());
            _settings.Load();
            _output = new RecordingAudioOutput();
            _audio = new AudioService(_config, _settings, _output);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_tapClip);
        }

        [Test]
        public void Play_ShouldPlayTheCuesClipAtItsVolume_WhenSoundIsOn()
        {
            _audio.Play(AudioCue.Tap);

            Assert.That(_output.Clips, Is.EqualTo(new[] { _tapClip }));
            Assert.That(_output.Volumes, Is.EqualTo(new[] { 0.4f }));
        }

        [Test]
        public void Play_ShouldStaySilent_WhenSoundIsOff()
        {
            _settings.Set(GameSetting.Sound, false);

            _audio.Play(AudioCue.Tap);

            Assert.That(_output.Clips, Is.Empty);
        }

        [Test]
        public void Play_ShouldThrow_WhenTheCueHasNoClip()
        {
            Assert.Throws<InvalidOperationException>(() => _audio.Play(AudioCue.Win));
        }

        [Test]
        public void ShippedConfig_ShouldHoldAClipForEveryCue_WhenTheGameShips()
        {
            var shipped = AssetDatabase.LoadAssetAtPath<AudioConfig>(ShippedConfigPath);

            foreach (AudioCue cue in Enum.GetValues(typeof(AudioCue)))
                Assert.That(shipped.For(cue).Clip, Is.Not.Null, cue.ToString());
        }
    }
}
