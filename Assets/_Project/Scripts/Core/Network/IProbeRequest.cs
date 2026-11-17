using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Network
{
    /// <summary>One connectivity request. A seam so tests answer probes by hand instead of using the network.</summary>
    public interface IProbeRequest
    {
        UniTask<ProbeResult> SendAsync(string url, float timeoutSeconds, CancellationToken cancellation);
    }

    /// <summary>A probe's answer: whether it succeeded and the server's HTTP Date header (null if absent).</summary>
    public readonly struct ProbeResult
    {
        public readonly bool Succeeded;
        public readonly string DateHeader;

        public ProbeResult(bool succeeded, string dateHeader)
        {
            Succeeded = succeeded;
            DateHeader = dateHeader;
        }
    }
}
