using UnityEngine;

namespace RollicCase.Game.Configs
{
    [CreateAssetMenu(menuName = "RollicCase/Configs/Board View Config", fileName = "BoardViewConfig")]
    public sealed class BoardViewConfig : ScriptableObject
    {
        [Tooltip("World size of one board cell. 1 matches the 0.5 model import scale (art-pipeline.md §3).")]
        [SerializeField, Min(0.01f)]
        float _cellSize = 1f;

        [Tooltip("Height of the invisible collider that catches board touches.")]
        [SerializeField, Min(0.01f)]
        float _inputHitAreaHeight = 0.1f;

        public float CellSize => _cellSize;
        public float InputHitAreaHeight => _inputHitAreaHeight;
    }
}
