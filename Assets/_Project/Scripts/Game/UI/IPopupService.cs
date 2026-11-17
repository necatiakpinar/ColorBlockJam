namespace ColorBlockJam.Game.UI
{
    public interface IPopupService
    {
        bool IsOpen { get; }

        void Open(IPopup popup);

        void Close();
    }
}
