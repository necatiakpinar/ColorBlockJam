using System.Collections.Generic;
using UnityEngine;

namespace RollicCase.Game.Levels
{
    /// <summary>The levels the game plays, in order. Level number N is entry N - 1.</summary>
    [CreateAssetMenu(menuName = "RollicCase/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField]
        List<LevelDefinition> _levels = new();

        public IReadOnlyList<LevelDefinition> Levels => _levels;
    }
}
