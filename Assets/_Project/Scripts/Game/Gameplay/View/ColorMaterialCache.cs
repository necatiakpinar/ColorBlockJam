using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Game.Gameplay.View
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
        readonly float _whiten;
        readonly Dictionary<BlockColor, Material> _materials = new();

        /// <param name="whiten">How far each palette colour is pulled toward white (0 keeps it; ice uses more).</param>
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
