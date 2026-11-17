using System;
using ColorBlockJam.Core.Network;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class FakeTrustedClock : ITrustedClock
    {
        public DateTime UtcNow { get; set; } = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);
        public bool Known { get; set; } = true;

        public void Advance(TimeSpan by) => UtcNow += by;

        public bool TryGetUtcNow(out DateTime utcNow)
        {
            utcNow = UtcNow;
            return Known;
        }
    }
}
