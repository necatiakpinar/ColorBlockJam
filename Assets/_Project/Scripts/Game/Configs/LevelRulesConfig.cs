using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>Limits every level must respect; the validator and the level editor read them.</summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Level Rules Config", fileName = "LevelRulesConfig")]
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

        [Tooltip("Board states the solver may visit before it answers \"unknown\" (DA-32).")]
        [SerializeField, Min(100)]
        int _solverStateCap = 20000;

        void OnValidate()
        {
            _maxBoardSize = Mathf.Max(_maxBoardSize, _minBoardSize);
            _maxTimerSeconds = Mathf.Max(_maxTimerSeconds, _minTimerSeconds);
        }

        public int MinBoardSize => _minBoardSize;
        public int MaxBoardSize => _maxBoardSize;
        public int MinTimerSeconds => _minTimerSeconds;
        public int MaxTimerSeconds => _maxTimerSeconds;
        public int SolverStateCap => _solverStateCap;
    }
}
