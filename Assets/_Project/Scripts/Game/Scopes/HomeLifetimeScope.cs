using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Scopes
{
    public sealed class HomeLifetimeScope : LifetimeScope
    {
        [SerializeField]
        HomeView _homeView;

        [SerializeField]
        LivesBarView _livesBarView;

        [Tooltip("Full-screen layer above the Home canvas content that the popup prefabs are created under.")]
        [SerializeField]
        Transform _popupLayer;

        protected override void Configure(IContainerBuilder builder)
        {
            new HomeInstaller().Install(builder);
            // The popup prefabs are needed while this scope is configured, so they come from the root's configs now.
            var popups = Parent.Container.Resolve<HomePopupConfig>();

            builder.RegisterComponent(_homeView);
            builder.RegisterComponent(_livesBarView);
            builder.RegisterComponentInNewPrefab(popups.SettingsPopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(popups.OutOfLivesPopup, Lifetime.Scoped).UnderTransform(_popupLayer);

            builder.RegisterEntryPoint<HomeFlow>();
        }
    }
}
