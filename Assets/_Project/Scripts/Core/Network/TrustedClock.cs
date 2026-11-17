using System;
using System.Globalization;
using ColorBlockJam.Core.Clock;

namespace ColorBlockJam.Core.Network
{
    /// <summary>
    /// The last server time a probe brought, moved forward by the monotonic time since it arrived (DA-27).
    /// The server's Date header has one-second precision.
    /// </summary>
    public sealed class TrustedClock : ITrustedClock
    {
        readonly IGameClock _clock;
        DateTime _serverUtc;
        float _receivedAt;
        bool _hasTime;

        public TrustedClock(IGameClock clock)
        {
            _clock = clock;
        }

        /// <summary>Keeps <paramref name="httpDate"/> (RFC 1123) as the time now; false if it does not parse.</summary>
        public bool TryRecord(string httpDate)
        {
            if (string.IsNullOrEmpty(httpDate) || !DateTime.TryParseExact(httpDate, "r", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime serverUtc))
                return false;
            _serverUtc = serverUtc;
            _receivedAt = _clock.Now;
            _hasTime = true;
            return true;
        }

        public bool TryGetUtcNow(out DateTime utcNow)
        {
            utcNow = _hasTime ? _serverUtc.AddSeconds(_clock.Now - _receivedAt) : default;
            return _hasTime;
        }
    }
}
