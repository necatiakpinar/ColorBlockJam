using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Audio;

namespace ColorBlockJam.Game.Boot
{
    public sealed class LoadAudioStep : IBootStep
    {
        readonly AudioService _audio;

        public LoadAudioStep(AudioService audio)
        {
            _audio = audio;
        }

        public float Weight => 1f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken) => _audio.LoadAsync(cancellationToken);
    }
}
