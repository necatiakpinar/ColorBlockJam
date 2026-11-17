using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    public readonly struct BlockMoveAnimation
    {
        public TweenerCore<Vector3, Vector3, VectorOptions> Create(Transform target, float duration, Ease ease)
        {
            TweenerCore<Vector3, Vector3, VectorOptions> tween = target.DOLocalMove(target.localPosition, duration);
            tween.SetEase(ease).SetUpdate(true).SetAutoKill(false).SetLink(target.gameObject).Pause();
            return tween;
        }

        public void Retarget(TweenerCore<Vector3, Vector3, VectorOptions> tween, Vector3 localEnd)
        {
            tween.ChangeEndValue(localEnd, true).Restart();
        }
    }
}
