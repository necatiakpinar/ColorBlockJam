namespace ColorBlockJam.Game.UI
{
    /// <summary>Shows one popup at a time in the current scene.</summary>
    public interface IPopupService
    {
        bool IsOpen { get; }

        /// <exception cref="System.InvalidOperationException">A popup is already open.</exception>
        void Open(IPopup popup);

        /// <exception cref="System.InvalidOperationException">No popup is open.</exception>
        void Close();
    }
}
