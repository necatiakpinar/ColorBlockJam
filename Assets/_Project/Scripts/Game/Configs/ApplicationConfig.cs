using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Application Config", fileName = "ApplicationConfig")]
    public sealed class ApplicationConfig : ScriptableObject
    {
        [SerializeField, Min(30)]
        int _targetFrameRate = 60;

        [Header("DOTween capacity (avoids runtime pool growth)")]
        [SerializeField, Min(10)]
        int _tweenCapacity = 200;

        [SerializeField, Min(5)]
        int _sequenceCapacity = 50;

        public int TargetFrameRate => _targetFrameRate;
        public int TweenCapacity => _tweenCapacity;
        public int SequenceCapacity => _sequenceCapacity;
    }
}
