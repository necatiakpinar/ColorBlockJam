using ColorBlockJam.Game.UI;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Game.Installers
{
    public sealed class HomeInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IPopupService, PopupService>(Lifetime.Singleton);
        }
    }
}
