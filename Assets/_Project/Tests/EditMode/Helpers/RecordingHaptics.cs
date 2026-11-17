using System.Collections.Generic;
using ColorBlockJam.Core.Haptics;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    /// <summary>Records every haptic tap instead of vibrating.</summary>
    public sealed class RecordingHaptics : IHapticService
    {
        public readonly List<HapticImpact> Played = new();

        public void Play(HapticImpact impact) => Played.Add(impact);
    }
}
