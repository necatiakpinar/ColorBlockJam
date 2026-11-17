using System;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    [Serializable]
    public sealed class PlayerProgressData
    {
        public const int FirstLevel = 1;

        [SerializeField]
        int _currentLevel = FirstLevel;

        public int CurrentLevel
        {
            get => _currentLevel;
            set => _currentLevel = value;
        }
    }
}
