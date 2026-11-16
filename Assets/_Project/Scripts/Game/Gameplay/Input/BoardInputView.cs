using RollicCase.Core.Signals;
using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay.Board;
using RollicCase.Game.Gameplay.Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace RollicCase.Game.Gameplay.Input
{
    /// <summary>
    /// Turns EventSystem pointer events on the board collider into <see cref="BoardPointerSignal"/>s.
    /// One finger at a time (DA-4). UI drawn above the board wins the EventSystem raycast, so touches on
    /// HUD buttons never arrive here.
    /// </summary>
    public sealed class BoardInputView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField]
        BoxCollider _hitArea;

        Camera _camera;
        BoardSpace _boardSpace;
        BoardViewConfig _config;
        SignalBus _signalBus;
        bool _isPressed;
        int _pointerId;
        Vector3 _lastWorldPoint;

        [Inject]
        public void Construct(Camera boardCamera, BoardSpace boardSpace, BoardViewConfig config, SignalBus signalBus)
        {
            _camera = boardCamera;
            _boardSpace = boardSpace;
            _config = config;
            _signalBus = signalBus;
        }

        /// <summary>Sizes the touch area to the current board; call after <see cref="BoardSpace.SetSize"/>.</summary>
        public void FitToBoard()
        {
            Vector3 size = _boardSpace.WorldSize;
            _hitArea.center = Vector3.zero;
            _hitArea.size = new Vector3(size.x, _config.InputHitAreaHeight, size.z);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPressed || !TryProject(eventData.position))
                return;

            _isPressed = true;
            _pointerId = eventData.pointerId;
            Fire(BoardPointerPhase.Pressed);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isPressed && eventData.pointerId == _pointerId && TryProject(eventData.position))
                Fire(BoardPointerPhase.Dragged);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed || eventData.pointerId != _pointerId)
                return;

            _isPressed = false;
            // A release that misses the board plane still ends the touch, at the last point it reached.
            TryProject(eventData.position);
            Fire(BoardPointerPhase.Released);
        }

        void OnDisable()
        {
            // The EventSystem sends no pointer-up to a disabled object; a stale press would block every
            // later touch.
            _isPressed = false;
        }

        bool TryProject(Vector2 screenPosition)
        {
            // Projecting on the board plane, not the collider, keeps drags valid past the board edge.
            if (!_boardSpace.TryProjectToBoard(_camera.ScreenPointToRay(screenPosition), out Vector3 worldPoint))
                return false;

            _lastWorldPoint = worldPoint;
            return true;
        }

        void Fire(BoardPointerPhase phase)
        {
            _signalBus.Fire(new BoardPointerSignal(phase, _boardSpace.WorldToCell(_lastWorldPoint), _lastWorldPoint));
        }
    }
}
