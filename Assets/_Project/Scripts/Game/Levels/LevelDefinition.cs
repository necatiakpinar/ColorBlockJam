using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Level Definition", fileName = "Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField]
        LevelData _data;

        public LevelData Data => _data;

        public void SetData(LevelData data)
        {
            _data = data;
        }
    }
}
