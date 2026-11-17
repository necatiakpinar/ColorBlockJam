using System;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Blockers
{
    /// <summary>
    /// The ice blocker as authored (T-34, DA-37): the block is frozen until <see cref="Exits"/> other blocks have left
    /// the board. 0 means the block has no ice.
    /// </summary>
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
