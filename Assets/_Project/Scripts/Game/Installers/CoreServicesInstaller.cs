using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Lives;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Packs;
using ColorBlockJam.Game.Navigation;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Installers
{
    public sealed class CoreServicesInstaller : IInstaller
    {
        readonly ConfigProvider _configs;

        public CoreServicesInstaller(ConfigProvider configs)
        {
            _configs = configs;
        }

        public void Install(IContainerBuilder builder)
        {
            new ConfigInstaller(_configs).Install(builder);

            builder.Register<SignalBus>(Lifetime.Singleton);
            builder.Register<IGameLogger, UnityGameLogger>(Lifetime.Singleton);
            builder.Register<IKeyValueStorage, PlayerPrefsStorage>(Lifetime.Singleton);
            builder.Register<IPlayerProgressService, PlayerProgressService>(Lifetime.Singleton);
            builder.Register<IWalletService, WalletService>(Lifetime.Singleton);
            builder.Register<ISettingsService, SettingsService>(Lifetime.Singleton);
            builder.Register<BoosterInventory>(Lifetime.Singleton);
            builder.Register<LivesService>(Lifetime.Singleton);
            builder.Register<LevelPackService>(Lifetime.Singleton).As<ILevelSource>().AsSelf();
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
            builder.Register<INavigationService, NavigationService>(Lifetime.Singleton);
            builder.Register<IHapticService, MobileHapticService>(Lifetime.Singleton);
            builder.Register<IGameClock, RealtimeGameClock>(Lifetime.Singleton);
            builder.Register<IProbeRequest, UnityWebRequestProbe>(Lifetime.Singleton);
            builder.Register<TrustedClock>(Lifetime.Singleton).As<ITrustedClock>().AsSelf();
            builder.Register(resolver => new ConnectivityService(resolver.Resolve<IProbeRequest>(),
                    resolver.Resolve<IGameClock>(), resolver.Resolve<TrustedClock>(), resolver.Resolve<SignalBus>(),
                    resolver.Resolve<IGameLogger>(), resolver.Resolve<NetworkConfig>().Connectivity), Lifetime.Singleton)
                .As<IConnectivityService>().AsSelf();
        }
    }
}
