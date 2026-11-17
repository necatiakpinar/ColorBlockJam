using DG.Tweening;

namespace ColorBlockJam.Core.Animation
{
    public interface IAnimation<TConfig> where TConfig : struct
    {
        Sequence Play(in TConfig config);
    }
}
