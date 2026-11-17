using System.Collections.Generic;
using ColorBlockJam.Game.Audio;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class RecordingAudio : IAudioService
    {
        public readonly List<AudioCue> Played = new();

        public void Play(AudioCue cue) => Played.Add(cue);
    }
}
