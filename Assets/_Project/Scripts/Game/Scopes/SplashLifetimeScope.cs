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
        SplashView _splashView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_splashView);
            new SplashInstaller().Install(builder);

            builder.RegisterEntryPoint<SplashFlow>();
        }
    }
}
