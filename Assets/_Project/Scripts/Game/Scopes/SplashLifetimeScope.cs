using RollicCase.Game.Configs;
using RollicCase.Game.Installers;
using RollicCase.Game.Splash;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Scopes
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
