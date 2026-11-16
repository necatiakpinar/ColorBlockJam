using RollicCase.Core.Logging;
using RollicCase.Core.Persistence;
using RollicCase.Core.Scenes;
using RollicCase.Core.Signals;
using RollicCase.Game.Configs;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Installers
{
    /// <summary>
    /// App-wide services that live for the whole session. Installers hold plain services only, so tests
    /// can build the same container without a scene; entry points are registered by the scope.
    /// </summary>
    public sealed class CoreServicesInstaller : IInstaller
    {
        readonly ApplicationConfig _applicationConfig;
        readonly UiConfig _uiConfig;

        public CoreServicesInstaller(ApplicationConfig applicationConfig, UiConfig uiConfig)
        {
            _applicationConfig = applicationConfig;
            _uiConfig = uiConfig;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_applicationConfig);
            builder.RegisterInstance(_uiConfig);

            builder.Register<SignalBus>(Lifetime.Singleton);
            builder.Register<IGameLogger, UnityGameLogger>(Lifetime.Singleton);
            builder.Register<IKeyValueStorage, PlayerPrefsStorage>(Lifetime.Singleton);
            builder.Register<IPlayerProgressService, PlayerProgressService>(Lifetime.Singleton);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
        }
    }
}
