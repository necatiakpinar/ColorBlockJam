using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using UnityEditor;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class BoardViewConfigTests
    {
        const string ShippedConfigPath = "Assets/_Project/Configs/BoardViewConfig.asset";

        static BoardViewConfig Shipped() => AssetDatabase.LoadAssetAtPath<BoardViewConfig>(ShippedConfigPath);

        [Test]
        public void ShippedConfig_ShouldCatchUpWithTheFingerWithinAFewFrames_WhenABlockIsHeld()
        {
            Assert.That(Shipped().FollowDuration, Is.EqualTo(0.089f).Within(0.0005f));
        }

        [Test]
        public void ExitSlideSeconds_ShouldCoverTheBlockPastTheCutAndClearance_WhenComputed()
        {
            BoardViewConfig config = Shipped();
            float expected = (2 + config.ExitCutOffset + config.ExitClearance) / config.ExitSpeed;

            Assert.That(config.ExitSlideDistance(2), Is.EqualTo(2 + config.ExitCutOffset + config.ExitClearance));
            Assert.That(config.ExitSlideSeconds(2), Is.EqualTo(expected));
            Assert.That(config.ExitSlideSeconds(3), Is.GreaterThan(config.ExitSlideSeconds(2)));
        }
    }
}
