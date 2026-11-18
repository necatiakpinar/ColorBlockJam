using ColorBlockJam.Core.Assets;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Game.Audio;
using ColorBlockJam.Game.Boot;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Installers;
using ColorBlockJam.Game.Lives;
using ColorBlockJam.Game.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Scopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [Tooltip("Every config of the game; each one is registered under its own type.")]
        [SerializeField]
        ConfigProvider _configs;

        [SerializeField]
        ApplicationPauseReporter _pauseReporter;

        [Tooltip("The one 2D audio source every sound effect plays through, with the session's audio listener.")]
        [SerializeField]
        SfxPlayer _sfxPlayer;

        [Tooltip("The black layer that hides every scene change.")]
        [SerializeField]
        ScreenFaderView _screenFader;

        protected override void Configure(IContainerBuilder builder)
        {
            new CoreServicesInstaller(_configs).Install(builder);
            builder.RegisterComponent(_pauseReporter);
            builder.RegisterComponent(_sfxPlayer).As<IAudioOutput>();
            builder.RegisterComponent(_screenFader).As<IScreenFader>();

            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<SpriteAtlasBinder>();
                resolver.Resolve<ConnectivityService>().Start();
            });

            builder.RegisterEntryPoint<TweeningInitializer>();
            builder.RegisterEntryPoint<LivesClock>();
        }
    }
}
