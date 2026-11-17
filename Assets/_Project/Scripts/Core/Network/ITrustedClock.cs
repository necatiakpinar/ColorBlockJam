using System;

namespace ColorBlockJam.Core.Network
{
    public interface ITrustedClock
    {
        bool TryGetUtcNow(out DateTime utcNow);
    }
}
