using NUnit.Framework;
using ColorBlockJam.Game.UI;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class SafeAreaTests
    {
        [Test]
        public void Anchors_ShouldCoverWholeScreen_WhenSafeAreaIsFullScreen()
        {
            var area = new SafeArea(new Rect(0f, 0f, 1080f, 1920f), new Vector2(1080f, 1920f));

            Assert.That(area.AnchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(area.AnchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void Anchors_ShouldExcludeNotchAndHomeBar_WhenTallPhoneHasInsets()
        {
            // 20:9 phone with a 100 px notch on top and a 60 px home bar at the bottom.
            var area = new SafeArea(new Rect(0f, 60f, 1080f, 2240f), new Vector2(1080f, 2400f));

            Assert.That(area.AnchorMin.y, Is.EqualTo(60f / 2400f).Within(1e-5f));
            Assert.That(area.AnchorMax.y, Is.EqualTo(2300f / 2400f).Within(1e-5f));
            Assert.That(area.AnchorMin.x, Is.Zero);
            Assert.That(area.AnchorMax.x, Is.EqualTo(1f));
        }
    }
}
