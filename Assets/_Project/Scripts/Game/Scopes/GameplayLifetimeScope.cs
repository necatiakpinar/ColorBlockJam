using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay;
using RollicCase.Game.Gameplay.Hud;
using RollicCase.Game.Gameplay.Input;
using RollicCase.Game.Installers;
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
        BoardViewConfig _boardViewConfig;

        [SerializeField]
        Camera _boardCamera;

        [SerializeField]
        GameplayHudView _hudView;

        [SerializeField]
        BoardInputView _boardInputView;

        protected override void Configure(IContainerBuilder builder)
        {
            new GameplayInstaller(_gameplayConfig, _boardViewConfig).Install(builder);

            // Components are injected while the container builds, so the HUD has subscribed to its
            // signals before any entry point below starts and fires one.
            builder.RegisterComponent(_boardCamera);
            builder.RegisterComponent(_hudView);
            builder.RegisterComponent(_boardInputView);

            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}
