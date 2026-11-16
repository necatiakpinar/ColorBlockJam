using UnityEngine;

namespace RollicCase.Game.Configs
{
    /// <summary>Limits every level must respect; the validator and the level editor read them.</summary>
    [CreateAssetMenu(menuName = "RollicCase/Configs/Level Rules Config", fileName = "LevelRulesConfig")]
    public sealed class LevelRulesConfig : ScriptableObject
    {
        [SerializeField, Min(1)]
        int _minBoardSize = 3;

        [Tooltip("Largest width or height; bigger boards no longer fit the portrait screen at a readable size.")]
        [SerializeField, Min(1)]
        int _maxBoardSize = 10;

        [SerializeField, Min(1)]
        int _minTimerSeconds = 10;

        [SerializeField, Min(1)]
        int _maxTimerSeconds = 600;

        void OnValidate()
        {
            _maxBoardSize = Mathf.Max(_maxBoardSize, _minBoardSize);
            _maxTimerSeconds = Mathf.Max(_maxTimerSeconds, _minTimerSeconds);
        }

        public int MinBoardSize => _minBoardSize;
        public int MaxBoardSize => _maxBoardSize;
        public int MinTimerSeconds => _minTimerSeconds;
        public int MaxTimerSeconds => _maxTimerSeconds;
    }
}
