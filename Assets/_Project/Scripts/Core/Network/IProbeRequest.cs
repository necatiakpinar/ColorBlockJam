using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Network
{
    public interface IProbeRequest
    {
        UniTask<ProbeResult> SendAsync(string url, float timeoutSeconds, CancellationToken cancellation);
    }

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
