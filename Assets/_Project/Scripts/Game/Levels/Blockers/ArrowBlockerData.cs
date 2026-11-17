using System;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Blockers
{
    [Serializable]
    public sealed class ArrowBlockerData
    {
        [Tooltip("The one axis the block may move along; None = no arrow.")]
        [SerializeField]
        ArrowAxis _axis;

        public ArrowBlockerData()
        {
        }

        public ArrowBlockerData(ArrowAxis axis) => _axis = axis;

        public ArrowAxis Axis
        {
            get => _axis;
            set => _axis = value;
        }

        public bool IsActive => _axis != ArrowAxis.None;
    }
}
