using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ColorBlockJam.Game.Audio
{
    [Serializable]
    public sealed class AudioCueSettings
    {
        [SerializeField]
        AudioCue _cue;

        [Tooltip("The cue's addressable clip, loaded at boot.")]
        [SerializeField]
        AssetReferenceT<AudioClip> _clip;

        [SerializeField, Range(0f, 1f)]
        float _volume = 1f;

        public AudioCue Cue => _cue;
        public AssetReferenceT<AudioClip> Clip => _clip;
        public float Volume => _volume;
    }
}
