using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ColorBlockJam.Core.Network
{
    /// <summary>A real probe: one GET; any 2xx answer is a success.</summary>
    public sealed class UnityWebRequestProbe : IProbeRequest
    {
        public async UniTask<ProbeResult> SendAsync(string url, float timeoutSeconds, CancellationToken cancellation)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = Mathf.CeilToInt(timeoutSeconds);
            try
            {
                await request.SendWebRequest().WithCancellation(cancellation);
            }
            catch (UnityWebRequestException)
            {
                // No connection, a timeout or an error status: an expected answer meaning "offline".
                return new ProbeResult(false, null);
            }

            bool succeeded = request.responseCode >= 200 && request.responseCode < 300;
            return new ProbeResult(succeeded, request.GetResponseHeader("Date"));
        }
    }
}
