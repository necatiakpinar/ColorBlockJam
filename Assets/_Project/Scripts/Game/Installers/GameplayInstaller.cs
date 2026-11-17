using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Feedback;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.UI;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Installers
{
    /// <summary>Gameplay-scene services.</summary>
    public sealed class GameplayInstaller : IInstaller
    {
        readonly GameplayConfig _gameplayConfig;
        readonly BoardViewConfig _boardViewConfig;

        public GameplayInstaller(GameplayConfig gameplayConfig, BoardViewConfig boardViewConfig)
        {
            _gameplayConfig = gameplayConfig;
            _boardViewConfig = boardViewConfig;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplayConfig);
            builder.RegisterInstance(_boardViewConfig);

            builder.Register(_ => new BoardSpace(_boardViewConfig.CellSize), Lifetime.Singleton);
            builder.Register<LevelProvider>(Lifetime.Singleton);
            builder.Register<ActiveBoard>(Lifetime.Singleton);
            builder.Register<LevelSession>(Lifetime.Singleton);
            builder.Register<LevelProgression>(Lifetime.Singleton);
            builder.Register<BlockDragController>(Lifetime.Singleton);
            builder.Register<GameplayHaptics>(Lifetime.Singleton);
            builder.Register<IPopupService, PopupService>(Lifetime.Singleton);
        }
    }
}
