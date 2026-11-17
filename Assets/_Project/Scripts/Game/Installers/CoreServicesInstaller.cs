using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Navigation;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Installers
{
    /// <summary>
    /// App-wide services that live for the whole session. Installers hold plain services only, so tests
    /// can build the same container without a scene; entry points are registered by the scope.
    /// </summary>
    public sealed class CoreServicesInstaller : IInstaller
    {
        readonly ApplicationConfig _applicationConfig;
        readonly UiConfig _uiConfig;
        readonly LevelCatalog _levelCatalog;
        readonly NetworkConfig _networkConfig;
        readonly BoosterConfig _boosterConfig;

        public CoreServicesInstaller(ApplicationConfig applicationConfig, UiConfig uiConfig, LevelCatalog levelCatalog,
            NetworkConfig networkConfig, BoosterConfig boosterConfig)
        {
            _applicationConfig = applicationConfig;
            _uiConfig = uiConfig;
            _levelCatalog = levelCatalog;
            _networkConfig = networkConfig;
            _boosterConfig = boosterConfig;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_applicationConfig);
            builder.RegisterInstance(_uiConfig);
            builder.RegisterInstance(_levelCatalog);
            builder.RegisterInstance(_networkConfig);
            builder.RegisterInstance(_boosterConfig);

            builder.Register<SignalBus>(Lifetime.Singleton);
            builder.Register<IGameLogger, UnityGameLogger>(Lifetime.Singleton);
            builder.Register<IKeyValueStorage, PlayerPrefsStorage>(Lifetime.Singleton);
            builder.Register<IPlayerProgressService, PlayerProgressService>(Lifetime.Singleton);
            builder.Register<IWalletService, WalletService>(Lifetime.Singleton);
            builder.Register<ISettingsService, SettingsService>(Lifetime.Singleton);
            builder.Register<BoosterInventory>(Lifetime.Singleton);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
            builder.Register<INavigationService, NavigationService>(Lifetime.Singleton);
            builder.Register<IHapticService, MobileHapticService>(Lifetime.Singleton);
            builder.Register<IGameClock, RealtimeGameClock>(Lifetime.Singleton);
            builder.Register<IProbeRequest, UnityWebRequestProbe>(Lifetime.Singleton);
            builder.Register<TrustedClock>(Lifetime.Singleton).As<ITrustedClock>().AsSelf();
            builder.Register(resolver => new ConnectivityService(resolver.Resolve<IProbeRequest>(),
                    resolver.Resolve<IGameClock>(), resolver.Resolve<TrustedClock>(), resolver.Resolve<SignalBus>(),
                    resolver.Resolve<IGameLogger>(), _networkConfig.Connectivity), Lifetime.Singleton)
                .As<IConnectivityService>().AsSelf();
        }
    }
}
