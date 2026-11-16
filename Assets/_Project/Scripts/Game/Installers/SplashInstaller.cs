using RollicCase.Core.Boot;
using RollicCase.Game.Boot;
using RollicCase.Game.Configs;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Installers
{
    /// <summary>
    /// Splash-scene services. Boot steps run in the order they are registered here.
    /// </summary>
    public sealed class SplashInstaller : IInstaller
    {
        readonly SplashConfig _splashConfig;

        public SplashInstaller(SplashConfig splashConfig)
        {
            _splashConfig = splashConfig;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_splashConfig);

            builder.Register<IBootStep, ApplyApplicationSettingsStep>(Lifetime.Singleton);
            builder.Register<IBootStep, LoadPlayerProgressStep>(Lifetime.Singleton);
            builder.Register<BootSequence>(Lifetime.Singleton);
        }
    }
}
