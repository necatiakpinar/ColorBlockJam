using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>
    /// Every config asset of the game in one list (architecture.md §5). The root scope registers it and each config in
    /// it under its own type, so a class asks for the config it needs: <see cref="GetConfig{T}"/>, or constructor
    /// injection. A new config is just added to the list.
    /// </summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Config Provider", fileName = "ConfigProvider")]
    public sealed class ConfigProvider : ScriptableObject
    {
        [Tooltip("One asset per config type.")]
        [SerializeField]
        List<ScriptableObject> _configs = new();

        public IReadOnlyList<ScriptableObject> All => _configs;

        /// <summary>The config of type <typeparamref name="T"/>; throws when the list holds none.</summary>
        public T GetConfig<T>() where T : ScriptableObject
        {
            for (int i = 0; i < _configs.Count; i++)
                if (_configs[i] is T config)
                    return config;
            throw new InvalidOperationException($"{name} holds no {typeof(T).Name}; add it to the list.");
        }
    }
}
