using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Game.Navigation
{
    /// <summary>Routes between screens so flows never name scenes.</summary>
    public interface INavigationService
    {
        /// <summary>A scene is loading; a route asked for meanwhile is ignored (a double tap).</summary>
        bool IsLoading { get; }

        UniTask GoToHomeAsync();

        UniTask GoToGameplayAsync();
    }
}
