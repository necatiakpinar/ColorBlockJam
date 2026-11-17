using System;
using UnityEngine;

namespace ColorBlockJam.Game.Lives
{
    /// <summary>
    /// The saved lives (persistence.md): how many are missing, and when the next one comes back in trusted UTC
    /// ticks (0 = no countdown running). Storing the missing count makes a new player full.
    /// </summary>
    [Serializable]
    public sealed class LivesData
    {
        [SerializeField]
        int _missing;

        [SerializeField]
        long _nextLifeUtcTicks;

        public int Missing
        {
            get => _missing;
            set => _missing = value;
        }

        public long NextLifeUtcTicks
        {
            get => _nextLifeUtcTicks;
            set => _nextLifeUtcTicks = value;
        }
    }
}
