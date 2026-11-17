using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Levels.Packs;

namespace ColorBlockJam.Game.Boot
{
    /// <summary>Picks this launch's level pack, a stored remote one or the bundled one, before Home shows a level (level-packs.md).</summary>
    public sealed class LoadLevelsStep : IBootStep
    {
        readonly LevelPackService _levels;

        public LoadLevelsStep(LevelPackService levels)
        {
            _levels = levels;
        }

        public float Weight => 1f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            _levels.Load();
            return UniTask.CompletedTask;
        }
    }
}
