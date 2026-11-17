using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>The lives system's tunables (lives.md, DA-38).</summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Lives Config", fileName = "LivesConfig")]
    public sealed class LivesConfig : ScriptableObject
    {
        [Tooltip("Lives a player can hold; a new player starts full.")]
        [SerializeField, Min(1)]
        int _maxLives = 5;

        [Tooltip("Minutes of trusted real time one missing life takes to come back.")]
        [SerializeField, Min(1)]
        int _refillMinutes = 30;

        [Tooltip("Coins that fill every missing life at once.")]
        [SerializeField, Min(1)]
        int _refillPrice = 100;

        public int MaxLives => _maxLives;
        public int RefillSeconds => _refillMinutes * 60;
        public int RefillPrice => _refillPrice;
    }
}
