using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Game.Gameplay.Hud;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    /// <summary>Records the coin flight; a flight lasts until <see cref="Land"/>, so tests see what waits for it.</summary>
    public sealed class RecordingCoinFlight : ICoinFlight
    {
        UniTaskCompletionSource _landing;

        public int? HeldAt { get; private set; }
        public List<int> Flights { get; } = new();

        public void HoldCounter(int shownCoins) => HeldAt = shownCoins;

        public UniTask FlyAsync(IReadOnlyList<RectTransform> sources, int coins, CancellationToken cancellation)
        {
            Flights.Add(coins);
            _landing = new UniTaskCompletionSource();
            return _landing.Task;
        }

        /// <summary>The last coin lands.</summary>
        public void Land() => _landing.TrySetResult();
    }
}
