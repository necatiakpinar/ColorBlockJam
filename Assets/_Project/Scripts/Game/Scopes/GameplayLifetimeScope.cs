using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay;
using RollicCase.Game.Gameplay.Hud;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Scopes
{
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField]
        GameplayConfig _gameplayConfig;

        [SerializeField]
        GameplayHudView _hudView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplayConfig);

            // Components are injected while the container builds, so the HUD has subscribed to its
            // signals before any entry point below starts and fires one.
            builder.RegisterComponent(_hudView);

            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}
