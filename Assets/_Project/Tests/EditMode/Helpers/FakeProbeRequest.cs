using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Network;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class FakeProbeRequest : IProbeRequest
    {
        readonly Queue<UniTaskCompletionSource<ProbeResult>> _pending = new();

        public int Sent { get; private set; }

        public int Pending => _pending.Count;

        public UniTask<ProbeResult> SendAsync(string url, float timeoutSeconds, CancellationToken cancellation)
        {
            Sent++;
            var source = new UniTaskCompletionSource<ProbeResult>();
            cancellation.Register(() => source.TrySetCanceled());
            _pending.Enqueue(source);
            return source.Task;
        }

        public void Answer(bool succeeded, string dateHeader = null) =>
            _pending.Dequeue().TrySetResult(new ProbeResult(succeeded, dateHeader));
    }
}
