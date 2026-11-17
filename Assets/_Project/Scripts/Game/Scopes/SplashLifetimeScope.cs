using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Installers;
using ColorBlockJam.Game.Splash;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Scopes
{
    public sealed class SplashLifetimeScope : LifetimeScope
    {
        [SerializeField]
        SplashConfig _splashConfig;

        [SerializeField]
        SplashView _splashView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_splashView);
            new SplashInstaller(_splashConfig).Install(builder);

            builder.RegisterEntryPoint<SplashFlow>();
        }
    }
}
