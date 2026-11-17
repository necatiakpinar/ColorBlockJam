using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>The asset that stores one <see cref="LevelData"/>. Written by the level editor, read by the game.</summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Level Definition", fileName = "Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField]
        LevelData _data;

        public LevelData Data => _data;

        /// <summary>Replaces the stored level; the level editor records undo before calling it.</summary>
        public void SetData(LevelData data)
        {
            _data = data;
        }
    }
}
