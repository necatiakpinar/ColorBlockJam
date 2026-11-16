using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay.Board;
using RollicCase.Game.UI;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Installers
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
            builder.Register<IPopupService, PopupService>(Lifetime.Singleton);
        }
    }
}
