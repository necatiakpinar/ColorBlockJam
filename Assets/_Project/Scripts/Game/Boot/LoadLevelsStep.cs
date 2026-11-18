using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Levels.Packs;

namespace ColorBlockJam.Game.Boot
{
    public sealed class LoadLevelsStep : IBootStep
    {
        readonly LevelPackService _levels;

        public LoadLevelsStep(LevelPackService levels)
        {
            _levels = levels;
        }

        public float Weight => 1f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken) => _levels.LoadAsync(cancellationToken);
    }
}
