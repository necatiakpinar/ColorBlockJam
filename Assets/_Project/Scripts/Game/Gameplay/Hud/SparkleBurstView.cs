using DG.Tweening;
using ColorBlockJam.Game.Gameplay.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    public sealed class SparkleBurstView : MonoBehaviour
    {
        [SerializeField]
        Image[] _sparkles;

        [SerializeField]
        Image _glow;

        Sequence _burst;

        public bool IsPlaying => _burst != null && _burst.IsActive() && _burst.IsPlaying();

        public void Play(Vector3 position, float distance, float duration)
        {
            transform.position = position;
            gameObject.SetActive(true);
            _burst?.Kill();
            _burst = default(SparkleBurstAnimation).Play(new SparkleBurstAnimation.Config(_sparkles, _glow, distance, duration))
                .OnComplete(Hide);
        }

        void Hide() => gameObject.SetActive(false);

        void OnDestroy() => _burst?.Kill();
    }
}
