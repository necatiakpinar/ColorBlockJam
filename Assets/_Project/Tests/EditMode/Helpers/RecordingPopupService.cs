using System;
using System.Collections.Generic;
using ColorBlockJam.Game.UI;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class RecordingPopupService : IPopupService
    {
        public List<IPopup> Opened { get; } = new();
        public IPopup Current { get; private set; }

        public bool IsOpen => Current != null;

        public void Open(IPopup popup)
        {
            if (Current != null)
                throw new InvalidOperationException($"Cannot open {popup} while {Current} is open.");
            Current = popup;
            Opened.Add(popup);
        }

        public void Close()
        {
            if (Current == null)
                throw new InvalidOperationException("No popup is open.");
            Current = null;
        }
    }
}
