using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Game.Navigation
{
    public interface IScreenFader
    {
        UniTask FadeOutAsync();

        UniTask FadeInAsync();
    }
}
