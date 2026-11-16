using RollicCase.Game.UI;
using VContainer;
using VContainer.Unity;

namespace RollicCase.Game.Installers
{
    /// <summary>Home-scene services.</summary>
    public sealed class HomeInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IPopupService, PopupService>(Lifetime.Singleton);
        }
    }
}
