using System;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>Screen space the board must leave free, as viewport fractions (HUD on top, boosters below).</summary>
    [Serializable]
    public struct FramingMargins
    {
        [SerializeField, Range(0f, 0.45f)]
        float _side;

        [SerializeField, Range(0f, 0.45f)]
        float _top;

        [SerializeField, Range(0f, 0.45f)]
        float _bottom;

        public FramingMargins(float side, float top, float bottom)
        {
            _side = side;
            _top = top;
            _bottom = bottom;
        }

        public float Side => _side;
        public float Top => _top;
        public float Bottom => _bottom;
    }
}
