using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    public sealed class BoardInputTests
    {
        static readonly Vector2Int BoardSize = new(5, 7);
        static readonly Vector2Int ScreenCoveringBoardSize = new(60, 60);

        BoardInputView _input;
        BoardSpace _space;
        Camera _camera;
        SignalBus _signalBus;
        readonly List<BoardPointerSignal> _received = new();

        IEnumerator SetUpBoard(Vector2Int size)
        {
            yield return SceneTestUtility.BootToGameplay();

            IObjectResolver container = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;
            _space = container.Resolve<BoardSpace>();
            _camera = container.Resolve<Camera>();
            _signalBus = container.Resolve<SignalBus>();
            _input = SceneTestUtility.FindInActiveScene<BoardInputView>();

            _space.SetSize(size);
            _input.FitToBoard();
            yield return new WaitForFixedUpdate();
            _received.Clear();
            _signalBus.Subscribe<BoardPointerSignal>(_received.Add);
        }

        [TearDown]
        public void TearDown()
        {
            _signalBus?.Unsubscribe<BoardPointerSignal>(_received.Add);
        }

        Vector2 ScreenPointOf(Vector2Int cell) => _camera.WorldToScreenPoint(_space.CellToWorld(cell));

        PointerEventData PointerAt(Vector2Int cell, int pointerId)
        {
            return new PointerEventData(EventSystem.current) { position = ScreenPointOf(cell), pointerId = pointerId };
        }

        static List<RaycastResult> RaycastAll(Vector2 screenPoint)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPoint }, results);
            return results;
        }

        [UnityTest]
        public IEnumerator PressDragRelease_ShouldReportCellsInOrder_WhenOneFingerMovesAcrossTheBoard()
        {
            yield return SetUpBoard(BoardSize);

            _input.OnPointerDown(PointerAt(new Vector2Int(2, 3), 0));
            _input.OnDrag(PointerAt(new Vector2Int(3, 3), 0));
            _input.OnPointerUp(PointerAt(new Vector2Int(3, 4), 0));

            Assert.That(_received.ConvertAll(signal => signal.Phase), Is.EqualTo(new[]
            {
                BoardPointerPhase.Pressed, BoardPointerPhase.Dragged, BoardPointerPhase.Released,
            }));
            Assert.That(_received.ConvertAll(signal => signal.Cell), Is.EqualTo(new[]
            {
                new Vector2Int(2, 3), new Vector2Int(3, 3), new Vector2Int(3, 4),
            }));
        }

        [UnityTest]
        public IEnumerator SecondFinger_ShouldBeIgnored_WhenAFingerIsAlreadyDown()
        {
            yield return SetUpBoard(BoardSize);

            _input.OnPointerDown(PointerAt(new Vector2Int(0, 0), 0));
            _input.OnPointerDown(PointerAt(new Vector2Int(4, 6), 1));
            _input.OnDrag(PointerAt(new Vector2Int(4, 5), 1));
            _input.OnPointerUp(PointerAt(new Vector2Int(4, 5), 1));

            Assert.That(_received.Count, Is.EqualTo(1));
            Assert.That(_received[0].Cell, Is.EqualTo(Vector2Int.zero));
        }

        [UnityTest]
        public IEnumerator Press_ShouldBeAccepted_WhenTheViewWasDisabledMidTouch()
        {
            yield return SetUpBoard(BoardSize);
            _input.OnPointerDown(PointerAt(new Vector2Int(1, 1), 0));

            _input.gameObject.SetActive(false);
            _input.gameObject.SetActive(true);
            _input.OnPointerDown(PointerAt(new Vector2Int(2, 2), 1));

            Assert.That(_received.ConvertAll(signal => signal.Cell), Is.EqualTo(new[]
            {
                new Vector2Int(1, 1), new Vector2Int(2, 2),
            }));
        }

        [UnityTest]
        public IEnumerator Release_ShouldEndTheTouchAtTheLastCell_WhenTheReleaseRayMissesTheBoardPlane()
        {
            yield return SetUpBoard(BoardSize);
            var aboveHorizon = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(Screen.width * 0.5f, Screen.height * 10f),
                pointerId = 0,
            };

            _input.OnPointerDown(PointerAt(new Vector2Int(2, 2), 0));
            _input.OnPointerUp(aboveHorizon);

            Assert.That(_received.ConvertAll(signal => signal.Phase), Is.EqualTo(new[]
            {
                BoardPointerPhase.Pressed, BoardPointerPhase.Released,
            }));
            Assert.That(_received[1].Cell, Is.EqualTo(new Vector2Int(2, 2)));
        }

        [UnityTest]
        public IEnumerator EventSystemRaycast_ShouldHitTheBoard_WhenTouchingABoardCell()
        {
            yield return SetUpBoard(BoardSize);

            List<RaycastResult> results = RaycastAll(ScreenPointOf(new Vector2Int(1, 1)));

            Assert.That(results, Is.Not.Empty);
            Assert.That(results[0].gameObject.GetComponent<BoardInputView>(), Is.SameAs(_input));
        }

        [UnityTest]
        public IEnumerator EventSystemRaycast_ShouldHitTheHudButtonFirst_WhenTheButtonCoversTheBoard()
        {
            yield return SetUpBoard(ScreenCoveringBoardSize);
            var pauseButton = (RectTransform)SceneTestUtility.FindButton<GameplayHudView>("PauseButton").transform;
            Vector2 buttonCentre = RectTransformUtility.WorldToScreenPoint(null, pauseButton.TransformPoint(pauseButton.rect.center));

            List<RaycastResult> results = RaycastAll(buttonCentre);

            Assert.That(results[0].gameObject, Is.SameAs(pauseButton.gameObject));
            Assert.That(results.Exists(result => result.gameObject == _input.gameObject), Is.True,
                "The board should lie under the button for this test to mean anything.");
        }

        [UnityTest]
        public IEnumerator Background_ShouldCancelTheHeldPressAndAcceptTheNextOne_WhenTheAppPausesMidDrag()
        {
            yield return SetUpBoard(BoardSize);

            _input.OnPointerDown(PointerAt(new Vector2Int(2, 3), 0));
            _signalBus.Fire(new ApplicationPausedSignal(true));
            _signalBus.Fire(new ApplicationPausedSignal(false));
            _input.OnPointerDown(PointerAt(new Vector2Int(1, 1), 1));

            Assert.That(_received.ConvertAll(signal => signal.Phase), Is.EqualTo(new[]
            {
                BoardPointerPhase.Pressed, BoardPointerPhase.Cancelled, BoardPointerPhase.Pressed,
            }));
        }
    }
}
