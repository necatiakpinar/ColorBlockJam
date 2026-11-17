using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>
    /// The levels as authored in the level editor, in order; level number N is entry N - 1. The game plays the
    /// level pack the editor writes from it (level-packs.md).
    /// </summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject, ILevelSource
    {
        [SerializeField]
        List<LevelDefinition> _levels = new();

        public IReadOnlyList<LevelDefinition> Levels => _levels;

        public int LevelCount => _levels.Count;

        public LevelData GetLevel(int index) => _levels[index].Data;
    }
}
