using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Assets;
using ColorBlockJam.Core.Boot;

namespace ColorBlockJam.Game.Boot
{
    public sealed class InitializeAssetsStep : IBootStep
    {
        readonly IAssetSource _assets;

        public InitializeAssetsStep(IAssetSource assets)
        {
            _assets = assets;
        }

        public float Weight => 1f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken) => _assets.InitializeAsync(cancellationToken);
    }
}
