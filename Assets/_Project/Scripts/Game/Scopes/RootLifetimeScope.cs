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

            builder.RegisterBuildCallback(resolver => resolver.Resolve<ConnectivityService>().Start());

            builder.RegisterEntryPoint<TweeningInitializer>();
            builder.RegisterEntryPoint<LivesClock>();
        }
    }
}
