using UnityEngine;

namespace RollicCase.Game.Gameplay.Signals
{
    /// <summary>
    /// A touch on the board. <see cref="Cell"/> can lie outside the board while dragging; consumers
    /// check <c>BoardSpace.Contains</c>.
    /// </summary>
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
