using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Audio
{
    public sealed class AudioService : IAudioService
    {
        readonly AudioConfig _config;
        readonly ISettingsService _settings;
        readonly IAudioOutput _output;

        public AudioService(AudioConfig config, ISettingsService settings, IAudioOutput output)
        {
            _config = config;
            _settings = settings;
            _output = output;
        }

        public void Play(AudioCue cue)
        {
            if (!_settings.IsOn(GameSetting.Sound))
                return;
            AudioCueSettings settings = _config.For(cue);
            _output.Play(settings.Clip, settings.Volume);
        }
    }
}
