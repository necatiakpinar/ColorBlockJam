using System;
using UnityEngine;

namespace ColorBlockJam.Game.Audio
{
    [Serializable]
    public sealed class AudioCueSettings
    {
        [SerializeField]
        AudioCue _cue;

        [SerializeField]
        AudioClip _clip;

        [SerializeField, Range(0f, 1f)]
        float _volume = 1f;

        public AudioCue Cue => _cue;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
    }
}
