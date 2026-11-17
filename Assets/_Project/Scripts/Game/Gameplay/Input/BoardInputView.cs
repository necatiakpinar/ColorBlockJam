using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Input
{
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
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
        }

        void OnBoardLoaded(BoardLoadedSignal signal) => FitToBoard();

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
            TryProject(eventData.position);
            Fire(BoardPointerPhase.Released);
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
        }

        void OnDisable()
        {
            _isPressed = false;
        }

        bool TryProject(Vector2 screenPosition)
        {
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
