using UnityEngine;

namespace ColorBlockJam.Game.Audio
{
    public interface IAudioOutput
    {
        void Play(AudioClip clip, float volume);
    }
}
