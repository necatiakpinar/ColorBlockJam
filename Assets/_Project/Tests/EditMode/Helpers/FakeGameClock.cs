using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class FakeGameClock : IGameClock
    {
        readonly List<(float Due, UniTaskCompletionSource Source)> _waits = new();

        public float Now { get; private set; }

        public UniTask DelayAsync(float seconds, CancellationToken cancellation)
        {
            var source = new UniTaskCompletionSource();
            cancellation.Register(() => source.TrySetCanceled());
            _waits.Add((Now + seconds, source));
            return source.Task;
        }

        public void Advance(float seconds)
        {
            float target = Now + seconds;
            while (TryTakeNextDue(target, out float due, out UniTaskCompletionSource source))
            {
                Now = due;
                source.TrySetResult();
            }

            Now = target;
        }

        bool TryTakeNextDue(float target, out float due, out UniTaskCompletionSource source)
        {
            int next = -1;
            for (int i = 0; i < _waits.Count; i++)
                if (_waits[i].Due <= target && (next < 0 || _waits[i].Due < _waits[next].Due))
                    next = i;
            if (next < 0)
            {
                due = 0f;
                source = null;
                return false;
            }

            (due, source) = _waits[next];
            _waits.RemoveAt(next);
            return true;
        }
    }
}
