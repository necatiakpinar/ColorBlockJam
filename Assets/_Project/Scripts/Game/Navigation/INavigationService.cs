using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Game.Navigation
{
    public interface INavigationService
    {
        bool IsLoading { get; }

        UniTask GoToHomeAsync();

        UniTask GoToGameplayAsync();
    }
}
