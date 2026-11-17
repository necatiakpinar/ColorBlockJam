using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    public interface ICoinFlight
    {
        void HoldCounter(int shownCoins);

        UniTask FlyAsync(IReadOnlyList<RectTransform> sources, int coins, CancellationToken cancellation);
    }
}
