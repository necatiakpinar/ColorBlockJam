using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    /// <summary>
    /// The won coins flying to the HUD counter (T-51, DA-12). The wallet is paid at the win; this only shows it:
    /// the counter holds the old balance until the coins land and count it up.
    /// </summary>
    public interface ICoinFlight
    {
        /// <summary>Keeps the HUD coin counter at <paramref name="shownCoins"/> until the flight counts coins in.</summary>
        void HoldCounter(int shownCoins);

        /// <summary>
        /// Flies one coin from each source to the counter, adding <paramref name="coins"/> to it as they land;
        /// completes once the last one has landed.
        /// </summary>
        UniTask FlyAsync(IReadOnlyList<RectTransform> sources, int coins, CancellationToken cancellation);
    }
}
