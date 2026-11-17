using System;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    [Serializable]
    public sealed class DoorData
    {
        [SerializeField]
        WallSide _side;

        [SerializeField]
        int _start;

        [SerializeField]
        int _length;

        [SerializeField]
        BlockColor _color;

        public DoorData(WallSide side, int start, int length, BlockColor color)
        {
            _side = side;
            _start = start;
            _length = length;
            _color = color;
        }

        public WallSide Side => _side;
        public int Start => _start;
        public int Length => _length;
        public int End => _start + _length - 1;
        public BlockColor Color => _color;
    }
}
