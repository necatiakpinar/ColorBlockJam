using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Level Pack Config", fileName = "LevelPackConfig")]
    public sealed class LevelPackConfig : ScriptableObject
    {
        [Tooltip("levels.json, written by the level editor on every catalog change; addressable, read once at boot.")]
        [SerializeField]
        AssetReferenceT<TextAsset> _bundledPack;

        public AssetReferenceT<TextAsset> BundledPack => _bundledPack;
    }
}
