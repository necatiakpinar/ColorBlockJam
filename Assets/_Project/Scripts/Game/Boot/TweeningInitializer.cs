using DG.Tweening;
using ColorBlockJam.Game.Configs;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Game.Boot
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
            // Safe mode swallows exceptions inside tweens; keep it off wherever bugs must surface.
            DOTween.Init(recycleAllByDefault: false, useSafeMode: !Debug.isDebugBuild, LogBehaviour.ErrorsOnly)
                .SetCapacity(_config.TweenCapacity, _config.SequenceCapacity);
        }
    }
}
