using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Boot;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Installers
{
    /// <summary>
    /// Splash-scene services. Boot steps run in the order they are registered here.
    /// </summary>
    public sealed class SplashInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IBootStep, ApplyApplicationSettingsStep>(Lifetime.Singleton);
            builder.Register<IBootStep, LoadLevelsStep>(Lifetime.Singleton);
            builder.Register<IBootStep, LoadPlayerDataStep>(Lifetime.Singleton);
            builder.Register<BootSequence>(Lifetime.Singleton);
        }
    }
}
