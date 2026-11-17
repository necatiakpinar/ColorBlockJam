using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>Where the game's levels come from (level-packs.md): the pack built into the app and the rules a pack must pass.</summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Level Pack Config", fileName = "LevelPackConfig")]
    public sealed class LevelPackConfig : ScriptableObject
    {
        [Tooltip("levels.json, written by the level editor on every catalog change.")]
        [SerializeField]
        TextAsset _bundledPack;

        [Tooltip("The board and timer limits every level of a pack must respect.")]
        [SerializeField]
        LevelRulesConfig _rules;

        public TextAsset BundledPack => _bundledPack;
        public LevelRulesConfig Rules => _rules;
    }
}
