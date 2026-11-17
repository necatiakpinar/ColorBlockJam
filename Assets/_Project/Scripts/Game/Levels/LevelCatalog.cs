using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
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
