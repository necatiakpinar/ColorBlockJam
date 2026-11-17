using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Game.Gameplay.View
{
    public sealed class ColorMaterialCache : IDisposable
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        readonly Material _baseMaterial;
        readonly BoardViewConfig _config;
        readonly float _whiten;
        readonly Dictionary<BlockColor, Material> _materials = new();

        public ColorMaterialCache(Material baseMaterial, BoardViewConfig config, float whiten = 0f)
        {
            _baseMaterial = baseMaterial;
            _config = config;
            _whiten = whiten;
        }

        public Material Get(BlockColor color)
        {
            if (_materials.TryGetValue(color, out Material material))
                return material;

            material = new Material(_baseMaterial) { name = $"{_baseMaterial.name}_{color}" };
            material.SetColor(BaseColor, Color.Lerp(_config.TintOf(color), Color.white, _whiten));
            _materials.Add(color, material);
            return material;
        }

        public void Dispose()
        {
            foreach (Material material in _materials.Values)
                Object.Destroy(material);
            _materials.Clear();
        }
    }
}
