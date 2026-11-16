using DG.Tweening;
using RollicCase.Game.Configs;
using VContainer.Unity;

namespace RollicCase.Game.Boot
{
    /// <summary>
    /// Configures DOTween once, when the root scope is built — before any scene scope can start a tween,
    /// because the first tween would otherwise auto-initialize DOTween with default settings.
    /// </summary>
    public sealed class TweeningInitializer : IInitializable
    {
        readonly ApplicationConfig _config;

        public TweeningInitializer(ApplicationConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            DOTween.Init(recycleAllByDefault: false, useSafeMode: true, LogBehaviour.ErrorsOnly)
                .SetCapacity(_config.TweenCapacity, _config.SequenceCapacity);
        }
    }
}
