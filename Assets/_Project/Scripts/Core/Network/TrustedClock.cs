using System;
using System.Globalization;
using ColorBlockJam.Core.Clock;

namespace ColorBlockJam.Core.Network
{
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
