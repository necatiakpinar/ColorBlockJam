using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay;
using ColorBlockJam.Game.Gameplay.Boosters;
using ColorBlockJam.Game.Gameplay.Feedback;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.View;
using ColorBlockJam.Game.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Scopes
{
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField]
        GameplayConfig _gameplayConfig;

        [SerializeField]
        BoardViewConfig _boardViewConfig;

        [SerializeField]
        LevelRewardConfig _levelRewardConfig;

        [SerializeField]
        Camera _boardCamera;

        [SerializeField]
        GameplayHudView _hudView;

        [SerializeField]
        BoosterBarView _boosterBarView;

        [SerializeField]
        BoosterTargetingView _boosterTargetingView;

        [SerializeField]
        FreezeView _freezeView;

        [SerializeField]
        BoardInputView _boardInputView;

        [SerializeField]
        BoardView _boardView;

        [SerializeField]
        BlockLayerView _blockLayerView;

        [SerializeField]
        BoardCameraView _boardCameraView;

        [SerializeField]
        GameplayPopupConfig _popupConfig;

        [Tooltip("Full-screen layer above the HUD that the popup prefabs are created under.")]
        [SerializeField]
        Transform _popupLayer;

        protected override void Configure(IContainerBuilder builder)
        {
            new GameplayInstaller(_gameplayConfig, _boardViewConfig, _levelRewardConfig).Install(builder);

            // Components are injected while the container builds, so the HUD has subscribed to its
            // signals before any entry point below starts and fires one.
            builder.RegisterComponent(_boardCamera);
            builder.RegisterComponent(_hudView);
            builder.RegisterComponent(_boosterBarView);
            builder.RegisterComponent(_boosterTargetingView);
            builder.RegisterComponent(_freezeView);
            builder.RegisterComponent(_boardInputView);
            builder.RegisterComponent(_boardView);
            builder.RegisterComponent(_blockLayerView);
            builder.RegisterComponent(_boardCameraView);
            // Popups come from their prefabs (GameplayPopupConfig), created under the popup layer and injected.
            builder.RegisterComponentInNewPrefab(_popupConfig.PausePopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(_popupConfig.WinPopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(_popupConfig.OutOfTimePopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(_popupConfig.LevelFailedPopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(_popupConfig.BoosterPurchasePopup, Lifetime.Scoped).UnderTransform(_popupLayer);

            // Flows that only listen; they need the scene's popups, so the scope registers them, not the installer.
            builder.Register<WinFlow>(Lifetime.Singleton);
            builder.Register<FailFlow>(Lifetime.Singleton);
            builder.Register<BoosterFlow>(Lifetime.Singleton);

            // Listen to board signals without being entry points: build them before any entry point starts.
            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<BlockDragController>();
                resolver.Resolve<GameplayHaptics>();
                resolver.Resolve<LevelProgression>();
                resolver.Resolve<LevelTimer>();
                resolver.Resolve<LevelRewardService>();
                resolver.Resolve<WinFlow>();
                resolver.Resolve<FailFlow>();
                resolver.Resolve<BoosterService>();
                resolver.Resolve<BoosterFlow>();
            });

            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}
