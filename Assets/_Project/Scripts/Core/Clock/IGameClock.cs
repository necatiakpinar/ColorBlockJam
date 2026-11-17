using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Clock
{
    /// <summary>
    /// Real time for game rules that count it (a level timer). A seam so tests drive time by hand instead of
    /// waiting; the real one is <see cref="RealtimeGameClock"/>.
    /// </summary>
    public interface IGameClock
    {
        /// <summary>Seconds since some fixed moment; only differences between readings mean anything.</summary>
        float Now { get; }

        /// <summary>Completes after <paramref name="seconds"/> of real time; cancelled with the token.</summary>
        UniTask DelayAsync(float seconds, CancellationToken cancellation);
    }
}
