using System;
using System.Collections.Generic;
using RollicCase.Game.Configs;
using RollicCase.Game.Levels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RollicCase.Game.Gameplay.View
{
    /// <summary>
    /// One material per block colour, made from a base material and the palette on first use. Shared
    /// materials (rather than property blocks) keep same-coloured parts batched by the SRP batcher.
    /// </summary>
    public sealed class ColorMaterialCache : IDisposable
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        readonly Material _baseMaterial;
        readonly BoardViewConfig _config;
        readonly Dictionary<BlockColor, Material> _materials = new();

        public ColorMaterialCache(Material baseMaterial, BoardViewConfig config)
        {
            _baseMaterial = baseMaterial;
            _config = config;
        }

        public Material Get(BlockColor color)
        {
            if (_materials.TryGetValue(color, out Material material))
                return material;

            material = new Material(_baseMaterial) { name = $"{_baseMaterial.name}_{color}" };
            material.SetColor(BaseColor, _config.TintOf(color));
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
