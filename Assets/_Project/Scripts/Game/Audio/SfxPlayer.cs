using UnityEngine;

namespace ColorBlockJam.Game.Audio
{
    public sealed class SfxPlayer : MonoBehaviour, IAudioOutput
    {
        [Tooltip("2D source every effect plays through as a one-shot.")]
        [SerializeField]
        AudioSource _source;

        public void Play(AudioClip clip, float volume) => _source.PlayOneShot(clip, volume);
    }
}
