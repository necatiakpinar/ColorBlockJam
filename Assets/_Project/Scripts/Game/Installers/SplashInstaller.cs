using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Boot;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Installers
{
    public sealed class SplashInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IBootStep, InitializeAssetsStep>(Lifetime.Singleton);
            builder.Register<IBootStep, ApplyApplicationSettingsStep>(Lifetime.Singleton);
            builder.Register<IBootStep, LoadLevelsStep>(Lifetime.Singleton);
            builder.Register<IBootStep, LoadPlayerDataStep>(Lifetime.Singleton);
            builder.Register<BootSequence>(Lifetime.Singleton);
        }
    }
}
