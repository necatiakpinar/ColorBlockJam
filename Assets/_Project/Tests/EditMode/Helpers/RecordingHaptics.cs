using System.Collections.Generic;
using ColorBlockJam.Core.Haptics;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class RecordingHaptics : IHapticService
    {
        public readonly List<HapticImpact> Played = new();

        public void Play(HapticImpact impact) => Played.Add(impact);
    }
}
