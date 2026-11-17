using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.Core.Clock
{
    public sealed class RealtimeGameClock : IGameClock
    {
        public float Now => Time.realtimeSinceStartup;

        public UniTask DelayAsync(float seconds, CancellationToken cancellation) =>
            UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.Realtime, PlayerLoopTiming.Update, cancellation);
    }
}
