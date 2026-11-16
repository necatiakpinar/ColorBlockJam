using RollicCase.Game.Boot;
using RollicCase.Game.Configs;
using RollicCase.Game.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Scopes
{
    /// <summary>
    /// The app-wide scope. Spawned once from the prefab set in VContainerSettings and kept across scenes;
    /// every scene scope gets it as its parent automatically.
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField]
        ApplicationConfig _applicationConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            new CoreServicesInstaller(_applicationConfig).Install(builder);

            builder.RegisterEntryPoint<TweeningInitializer>();
        }
    }
}
