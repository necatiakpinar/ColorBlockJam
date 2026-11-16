using System;

namespace RollicCase.Game.UI
{
    public sealed class PopupService : IPopupService
    {
        IPopup _open;

        public bool IsOpen => _open != null;

        public void Open(IPopup popup)
        {
            if (popup == null)
                throw new ArgumentNullException(nameof(popup));
            // The open popup's blocker swallows taps behind it, so a second open is a wiring bug.
            if (_open != null)
                throw new InvalidOperationException($"Cannot open a popup while {_open} is open.");

            _open = popup;
            popup.Show();
        }

        public void Close()
        {
            if (_open == null)
                throw new InvalidOperationException("No popup is open.");

            IPopup closing = _open;
            _open = null;
            closing.Hide();
        }
    }
}
