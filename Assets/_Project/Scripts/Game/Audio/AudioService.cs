using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Assets;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Configs;
using UnityEngine;

namespace ColorBlockJam.Game.Audio
{
    public sealed class AudioService : IAudioService
    {
        readonly AudioConfig _config;
        readonly ISettingsService _settings;
        readonly IAudioOutput _output;
        readonly AssetLoader _assets;
        AudioClip[] _clips;

        public AudioService(AudioConfig config, ISettingsService settings, IAudioOutput output, AssetLoader assets)
        {
            _config = config;
            _settings = settings;
            _output = output;
            _assets = assets;
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<AudioCueSettings> cues = _config.Cues;
            var loads = new UniTask<AudioClip>[cues.Count];
            for (int i = 0; i < cues.Count; i++)
                loads[i] = _assets.LoadAsync(cues[i].Clip, cancellationToken);
            _clips = await UniTask.WhenAll(loads);
        }

        public void Play(AudioCue cue)
        {
            if (_clips == null)
                throw new InvalidOperationException("Load the sounds at boot before playing one.");
            if (!_settings.IsOn(GameSetting.Sound))
                return;
            int index = _config.IndexOf(cue);
            _output.Play(_clips[index], _config.Cues[index].Volume);
        }
    }
}
