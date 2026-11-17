using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Game.Navigation
{
    /// <summary>Routes between screens so flows never name scenes.</summary>
    public interface INavigationService
    {
        UniTask GoToHomeAsync();

        UniTask GoToGameplayAsync();
    }
}
