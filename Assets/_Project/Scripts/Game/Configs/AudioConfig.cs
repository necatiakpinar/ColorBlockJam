using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Audio;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [Tooltip("One entry per cue: the clip and its volume (DA-43).")]
        [SerializeField]
        List<AudioCueSettings> _cues = new();

        public IReadOnlyList<AudioCueSettings> Cues => _cues;

        public AudioCueSettings For(AudioCue cue) => _cues[IndexOf(cue)];

        public int IndexOf(AudioCue cue)
        {
            for (int i = 0; i < _cues.Count; i++)
                if (_cues[i].Cue == cue)
                    return i;
            throw new InvalidOperationException($"{name} has no clip for the {cue} cue.");
        }
    }
}
