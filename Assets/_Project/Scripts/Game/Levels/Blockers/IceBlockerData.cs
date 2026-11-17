using System;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Blockers
{
    [Serializable]
    public sealed class IceBlockerData
    {
        [Tooltip("Other blocks that must leave the board before this one thaws; 0 = no ice.")]
        [SerializeField, Min(0)]
        int _exits;

        public IceBlockerData()
        {
        }

        public IceBlockerData(int exits) => _exits = exits;

        public int Exits
        {
            get => _exits;
            set => _exits = value;
        }

        public bool IsActive => _exits > 0;
    }
}
