using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Clock
{
    public interface IGameClock
    {
        float Now { get; }

        UniTask DelayAsync(float seconds, CancellationToken cancellation);
    }
}
