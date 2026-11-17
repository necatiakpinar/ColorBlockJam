using System;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Blockers
{
    /// <summary>
    /// Everything a block carries besides its colour and cells (blockers.md): one entry per blocker kind, each inactive
    /// by default. A new blocker kind adds its data class here, its rule in <c>Gameplay/Board/Blockers</c>, its view
    /// prefab in <c>Prefabs/Board/Blockers</c> and its row in the level editor's Blockers panel.
    /// </summary>
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

        /// <summary>No blocker is active: a plain block.</summary>
        public bool IsEmpty => !_ice.IsActive && !_arrow.IsActive;

        /// <summary>A deep copy, so an edited level never shares its blockers with another.</summary>
        public BlockerSet Clone() => new(new IceBlockerData(_ice.Exits), new ArrowBlockerData(_arrow.Axis));
    }
}
