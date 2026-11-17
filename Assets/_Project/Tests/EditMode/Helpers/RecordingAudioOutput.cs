using System.Collections.Generic;
using ColorBlockJam.Game.Audio;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class RecordingAudioOutput : IAudioOutput
    {
        public readonly List<AudioClip> Clips = new();
        public readonly List<float> Volumes = new();

        public void Play(AudioClip clip, float volume)
        {
            Clips.Add(clip);
            Volumes.Add(volume);
        }
    }
}
