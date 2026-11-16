using RollicCase.Game.Home;
using RollicCase.Game.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Scopes
{
    public sealed class HomeLifetimeScope : LifetimeScope
    {
        [SerializeField]
        HomeView _homeView;

        protected override void Configure(IContainerBuilder builder)
        {
            new HomeInstaller().Install(builder);

            builder.RegisterComponent(_homeView);

            builder.RegisterEntryPoint<HomeFlow>();
        }
    }
}
