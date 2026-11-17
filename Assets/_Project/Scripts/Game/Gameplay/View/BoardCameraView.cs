using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// Moves the board camera so the whole board, walls included, fits between the HUD and the booster bar
    /// at the screen's aspect ratio when a board loads. The camera's tilt and field of view are authored in
    /// the scene. The aspect is fixed on device (portrait only), so there is no re-fit on resize.
    /// </summary>
    public sealed class BoardCameraView : MonoBehaviour
    {
        readonly BoardFraming _framing = new();
        Camera _camera;
        BoardSpace _boardSpace;
        BoardViewConfig _config;
        SignalBus _signalBus;

        [Inject]
        public void Construct(Camera boardCamera, BoardSpace boardSpace, BoardViewConfig config, SignalBus signalBus)
        {
            _camera = boardCamera;
            _boardSpace = boardSpace;
            _config = config;
            _signalBus = signalBus;
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
        }

        void OnBoardLoaded(BoardLoadedSignal signal) => Frame();

        void Frame()
        {
            float cell = _boardSpace.CellSize;
            float ring = _config.WallThickness * cell;
            Vector3 size = _boardSpace.WorldSize + new Vector3(ring * 2f, _config.FramedHeight * cell, ring * 2f);
            var box = new Bounds(new Vector3(0f, size.y * 0.5f, 0f), size);

            _camera.transform.position = _framing.FitPosition(
                box, _camera.transform.rotation, _camera.fieldOfView, _camera.aspect, _config.Framing);
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
        }
    }
}
