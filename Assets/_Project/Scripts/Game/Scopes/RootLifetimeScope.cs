using ColorBlockJam.Game.Boot;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Installers;
using ColorBlockJam.Game.Levels;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Scopes
{
    /// <summary>
    /// The app-wide scope. Spawned once from the prefab set in VContainerSettings and kept across scenes;
    /// every scene scope gets it as its parent automatically.
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField]
        ApplicationConfig _applicationConfig;

        [SerializeField]
        UiConfig _uiConfig;

        [SerializeField]
        LevelCatalog _levelCatalog;

        protected override void Configure(IContainerBuilder builder)
        {
            new CoreServicesInstaller(_applicationConfig, _uiConfig, _levelCatalog).Install(builder);

            builder.RegisterEntryPoint<TweeningInitializer>();
        }
    }
}
