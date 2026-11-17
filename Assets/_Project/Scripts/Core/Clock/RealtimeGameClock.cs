using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.Core.Clock
{
    /// <summary>Unscaled real time: a time scale never speeds up or freezes what it counts.</summary>
    public sealed class RealtimeGameClock : IGameClock
    {
        public float Now => Time.realtimeSinceStartup;

        public UniTask DelayAsync(float seconds, CancellationToken cancellation) =>
            UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.Realtime, PlayerLoopTiming.Update, cancellation);
    }
}
