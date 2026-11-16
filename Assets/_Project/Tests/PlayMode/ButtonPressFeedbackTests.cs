using System.Collections;
using NUnit.Framework;
using RollicCase.Game.Configs;
using RollicCase.Game.Home;
using RollicCase.Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;

namespace RollicCase.Tests.PlayMode
{
    /// <summary>A real Home button shrinks while pressed and returns to its size on release.</summary>
    public sealed class ButtonPressFeedbackTests
    {
        const float SettleMarginSeconds = 0.2f;

        Button _button;
        ButtonPressFeedback _feedback;
        PointerEventData _pointer;
        float _settleSeconds;

        IEnumerator SetUpLevelButton()
        {
            yield return SceneTestUtility.BootToHome();
            _button = SceneTestUtility.FindButton<HomeView>("LevelButton");
            _feedback = _button.GetComponent<ButtonPressFeedback>();
            _pointer = new PointerEventData(EventSystem.current);

            var uiConfig = SceneTestUtility.FindInActiveScene<RollicCase.Game.Scopes.HomeLifetimeScope>()
                .Container.Resolve<UiConfig>();
            _settleSeconds = Mathf.Max(uiConfig.PressFeedback.PressDuration, uiConfig.PressFeedback.ReleaseDuration)
                + SettleMarginSeconds;
        }

        static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

        float Scale => _button.transform.localScale.x;

        [UnityTest]
        public IEnumerator LevelButton_ShouldShrinkWhilePressedAndRestoreOnRelease_WhenTapped()
        {
            yield return SetUpLevelButton();

            _feedback.OnPointerDown(_pointer);
            yield return WaitRealtime(_settleSeconds);
            float pressedScale = Scale;
            _feedback.OnPointerUp(_pointer);
            yield return WaitRealtime(_settleSeconds);

            Assert.That(pressedScale, Is.LessThan(1f));
            Assert.That(Scale, Is.EqualTo(1f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator LevelButton_ShouldKeepItsSize_WhenNotInteractable()
        {
            yield return SetUpLevelButton();
            _button.interactable = false;

            _feedback.OnPointerDown(_pointer);
            yield return WaitRealtime(_settleSeconds);

            Assert.That(Scale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator LevelButton_ShouldRestoreItsSize_WhenDisabledWhilePressed()
        {
            yield return SetUpLevelButton();
            _feedback.OnPointerDown(_pointer);
            yield return WaitRealtime(_settleSeconds);

            _button.gameObject.SetActive(false);

            Assert.That(Scale, Is.EqualTo(1f));
        }
    }
}
