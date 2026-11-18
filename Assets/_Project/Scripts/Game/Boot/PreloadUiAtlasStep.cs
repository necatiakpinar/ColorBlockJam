using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Assets;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Boot
{
    public sealed class PreloadUiAtlasStep : IBootStep
    {
        readonly SpriteAtlasBinder _atlases;
        readonly UiConfig _config;

        public PreloadUiAtlasStep(SpriteAtlasBinder atlases, UiConfig config)
        {
            _atlases = atlases;
            _config = config;
        }

        public float Weight => 1f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken) => _atlases.PreloadAsync(_config.AtlasAddress, cancellationToken);
    }
}
