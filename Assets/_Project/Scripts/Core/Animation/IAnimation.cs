using DG.Tweening;

namespace RollicCase.Core.Animation
{
    /// <summary>
    /// A stateless motion recipe. <see cref="Play"/> builds a DOTween sequence purely from the config
    /// (which also carries the target); the caller owns the returned sequence and may kill it.
    /// </summary>
    public interface IAnimation<TConfig> where TConfig : struct
    {
        Sequence Play(in TConfig config);
    }
}
