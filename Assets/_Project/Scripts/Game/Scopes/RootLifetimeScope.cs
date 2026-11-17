using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Game.Boot;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Installers;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Lives;
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

        [SerializeField]
        NetworkConfig _networkConfig;

        [SerializeField]
        BoosterConfig _boosterConfig;

        [SerializeField]
        LivesConfig _livesConfig;

        [SerializeField]
        ApplicationPauseReporter _pauseReporter;

        protected override void Configure(IContainerBuilder builder)
        {
            new CoreServicesInstaller(_applicationConfig, _uiConfig, _levelCatalog, _networkConfig, _boosterConfig, _livesConfig)
                .Install(builder);
            builder.RegisterComponent(_pauseReporter);

            // Probing starts as soon as the app-wide container exists (online requirement, network.md).
            builder.RegisterBuildCallback(resolver => resolver.Resolve<ConnectivityService>().Start());

            builder.RegisterEntryPoint<TweeningInitializer>();
            // The lives' refill countdown runs for the whole session (lives.md).
            builder.RegisterEntryPoint<LivesClock>();
        }
    }
}
