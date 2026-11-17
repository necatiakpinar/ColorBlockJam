using System;
using ColorBlockJam.Game.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Installers
{
    public sealed class ConfigInstaller : IInstaller
    {
        readonly ConfigProvider _configs;

        public ConfigInstaller(ConfigProvider configs)
        {
            _configs = configs;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_configs);
            foreach (ScriptableObject config in _configs.All)
            {
                if (config == null)
                    throw new InvalidOperationException($"{_configs.name} has an empty entry in its config list.");
                builder.RegisterInstance(config).As(config.GetType());
            }
        }
    }
}
