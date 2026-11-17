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

        [Tooltip("The won coins' flight to the HUD counter (T-51).")]
        [SerializeField]
        CoinFlightView _coinFlight;

        [Tooltip("Full-screen layer above the HUD that the popup prefabs are created under.")]
        [SerializeField]
        Transform _popupLayer;

        protected override void Configure(IContainerBuilder builder)
        {
            new GameplayInstaller().Install(builder);
            var popups = Parent.Container.Resolve<GameplayPopupConfig>();

            builder.RegisterComponent(_boardCamera);
            builder.RegisterComponent(_hudView);
            builder.RegisterComponent(_boosterBarView);
            builder.RegisterComponent(_boosterTargetingView);
            builder.RegisterComponent(_freezeView);
            builder.RegisterComponent(_boardInputView);
            builder.RegisterComponent(_boardView);
            builder.RegisterComponent(_blockLayerView);
            builder.RegisterComponent(_boardCameraView);
            builder.RegisterComponent(_coinFlight).As<ICoinFlight>();
            builder.RegisterComponentInNewPrefab(popups.PausePopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(popups.WinPopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(popups.OutOfTimePopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(popups.LevelFailedPopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(popups.BoosterPurchasePopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(popups.OutOfLivesPopup, Lifetime.Scoped).UnderTransform(_popupLayer);
            builder.RegisterComponentInNewPrefab(popups.LeaveLevelPopup, Lifetime.Scoped).UnderTransform(_popupLayer);

            builder.Register<WinFlow>(Lifetime.Singleton);
            builder.Register<FailFlow>(Lifetime.Singleton);
            builder.Register<BoosterFlow>(Lifetime.Singleton);
            builder.Register<GameplayLivesFlow>(Lifetime.Singleton);

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
                resolver.Resolve<LevelAttemptLives>();
                resolver.Resolve<GameplayLivesFlow>();
            });

            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}
