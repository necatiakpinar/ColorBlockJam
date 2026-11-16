using RollicCase.Game.UI;

namespace RollicCase.Tests.EditMode.Helpers
{
    public sealed class RecordingPopup : IPopup
    {
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }

        public void Show() => ShowCount++;

        public void Hide() => HideCount++;
    }
}
