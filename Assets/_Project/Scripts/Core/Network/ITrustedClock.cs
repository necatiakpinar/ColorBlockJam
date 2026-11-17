using System;

namespace ColorBlockJam.Core.Network
{
    /// <summary>
    /// The current time as the internet tells it, for wall-clock features (lives refill, daily rewards). The
    /// device clock is never used, so moving it changes nothing (R-44, DA-27).
    /// </summary>
    public interface ITrustedClock
    {
        /// <summary>False until a probe has brought a server time; features that need it wait.</summary>
        bool TryGetUtcNow(out DateTime utcNow);
    }
}
