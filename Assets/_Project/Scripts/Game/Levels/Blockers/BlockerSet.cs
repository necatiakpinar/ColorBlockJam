using System;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Blockers
{
    [Serializable]
    public sealed class BlockerSet
    {
        [SerializeField]
        IceBlockerData _ice = new();

        [SerializeField]
        ArrowBlockerData _arrow = new();

        public BlockerSet()
        {
        }

        public BlockerSet(IceBlockerData ice = null, ArrowBlockerData arrow = null)
        {
            _ice = ice ?? new IceBlockerData();
            _arrow = arrow ?? new ArrowBlockerData();
        }

        public IceBlockerData Ice => _ice;
        public ArrowBlockerData Arrow => _arrow;

        public bool IsEmpty => !_ice.IsActive && !_arrow.IsActive;

        public BlockerSet Clone() => new(new IceBlockerData(_ice.Exits), new ArrowBlockerData(_arrow.Axis));
    }
}
