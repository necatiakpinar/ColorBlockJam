using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BoardPointerSignal
    {
        public readonly BoardPointerPhase Phase;
        public readonly Vector2Int Cell;
        public readonly Vector3 WorldPoint;

        public BoardPointerSignal(BoardPointerPhase phase, Vector2Int cell, Vector3 worldPoint)
        {
            Phase = phase;
            Cell = cell;
            WorldPoint = worldPoint;
        }
    }
}
