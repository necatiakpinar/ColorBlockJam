using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.UI;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace ColorBlockJam.Tests.PlayMode
{
    public sealed class HomeScreenTests
    {
        [UnityTest]
        public IEnumerator Home_ShouldShowTheCurrentLevelOnThePathAndTheButton_WhenItOpens()
        {
            yield return SceneTestUtility.BootToHome();
            HomeView home = SceneTestUtility.FindInActiveScene<HomeView>();

            Assert.That(home.LevelLabelText, Is.EqualTo("Level " + home.PathLabelText(0)));
            Assert.That(int.Parse(home.PathLabelText(1)), Is.EqualTo(int.Parse(home.PathLabelText(0)) + 1));
        }

        [UnityTest]
        public IEnumerator SettingsButton_ShouldOpenAndCloseTheSettingsPopup_WhenTapped()
        {
            yield return SceneTestUtility.BootToHome();

            SceneTestUtility.TapButton<HomeView>("SettingsButton");
            bool shown = SceneTestUtility.FindInActiveScene<SettingsPopupView>().gameObject.activeInHierarchy;
            SceneTestUtility.TapButton<SettingsPopupView>("CloseButton");

            Assert.That(shown, Is.True);
            Assert.That(SceneTestUtility.FindInActiveScene<SettingsPopupView>().gameObject.activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator LivesBar_ShouldOpenTheLivesPopup_WhenTappedAnywhere()
        {
            yield return SceneTestUtility.BootToHome();
            LivesBarView bar = SceneTestUtility.FindInActiveScene<LivesBarView>();

            bar.GetComponent<Button>().onClick.Invoke();

            Assert.That(bar.GetComponentsInChildren<Button>(true), Has.Length.EqualTo(1), "One button covers the bar.");
            Assert.That(SceneTestUtility.FindInActiveScene<OutOfLivesPopupView>().gameObject.activeInHierarchy, Is.True);
        }
    }
}
