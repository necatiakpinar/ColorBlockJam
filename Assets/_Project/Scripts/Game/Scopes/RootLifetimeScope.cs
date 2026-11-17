using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Game.Boot;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Installers;
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
        [Tooltip("Every config of the game; each one is registered under its own type.")]
        [SerializeField]
        ConfigProvider _configs;

        [SerializeField]
        ApplicationPauseReporter _pauseReporter;

        protected override void Configure(IContainerBuilder builder)
        {
            new CoreServicesInstaller(_configs).Install(builder);
            builder.RegisterComponent(_pauseReporter);

            // Probing starts as soon as the app-wide container exists (online requirement, network.md).
            builder.RegisterBuildCallback(resolver => resolver.Resolve<ConnectivityService>().Start());

            builder.RegisterEntryPoint<TweeningInitializer>();
            // The lives' refill countdown runs for the whole session (lives.md).
            builder.RegisterEntryPoint<LivesClock>();
        }
    }
}
