using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Animations
{
    /// <summary>
    /// The settle a block makes onto its cell when the finger lets go. Blocks are dropped again and again,
    /// so instead of building a sequence per drop it keeps one paused tween per block and retargets it:
    /// dropping allocates nothing. (The one exception to <c>IAnimation</c>'s build-per-play shape.)
    /// </summary>
    public readonly struct BlockMoveAnimation
    {
        /// <summary>Creates the block's reusable tween; it stays paused until <see cref="Retarget"/>.</summary>
        public TweenerCore<Vector3, Vector3, VectorOptions> Create(Transform target, float duration, Ease ease)
        {
            TweenerCore<Vector3, Vector3, VectorOptions> tween = target.DOLocalMove(target.localPosition, duration);
            tween.SetEase(ease).SetUpdate(true).SetAutoKill(false).SetLink(target.gameObject).Pause();
            return tween;
        }

        /// <summary>Restarts the tween from where the block is now toward <paramref name="localEnd"/>.</summary>
        public void Retarget(TweenerCore<Vector3, Vector3, VectorOptions> tween, Vector3 localEnd)
        {
            // The typed overload; the base Tweener one takes object and boxes the vector every call.
            tween.ChangeEndValue(localEnd, true).Restart();
        }
    }
}
