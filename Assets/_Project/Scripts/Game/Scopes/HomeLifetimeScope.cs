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

        [SerializeField]
        HomePopupConfig _popupConfig;

        [Tooltip("Full-screen layer above the Home canvas content that the popup prefabs are created under.")]
        [SerializeField]
        Transform _popupLayer;

        protected override void Configure(IContainerBuilder builder)
        {
            new HomeInstaller().Install(builder);

            builder.RegisterComponent(_homeView);
            builder.RegisterComponent(_livesBarView);
            builder.RegisterComponentInNewPrefab(_popupConfig.SettingsPopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(_popupConfig.OutOfLivesPopup, Lifetime.Scoped).UnderTransform(_popupLayer);

            builder.RegisterEntryPoint<HomeFlow>();
        }
    }
}
