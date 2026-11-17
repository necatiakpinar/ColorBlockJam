using DG.Tweening;
using ColorBlockJam.Game.Configs;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Game.Boot
{
    public sealed class TweeningInitializer : IInitializable
    {
        readonly ApplicationConfig _config;

        public TweeningInitializer(ApplicationConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            DOTween.Init(recycleAllByDefault: false, useSafeMode: !Debug.isDebugBuild, LogBehaviour.ErrorsOnly)
                .SetCapacity(_config.TweenCapacity, _config.SequenceCapacity);
        }
    }
}
