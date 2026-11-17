using System;
using UnityEngine;

namespace ColorBlockJam.Game.Lives
{
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
