using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.View;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class BoardFramingTests
    {
        static readonly Quaternion Tilt = Quaternion.Euler(60f, 0f, 0f);
        const float Fov = 40f;
        static readonly FramingMargins Margins = new(0.06f, 0.22f, 0.16f);

        /// <summary>Viewport position of a world point for a camera at <paramref name="position"/>.</summary>
        static Vector2 Viewport(Vector3 point, Vector3 position, float aspect)
        {
            Vector3 local = Quaternion.Inverse(Tilt) * (point - position);
            float tan = Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad);
            return new Vector2(0.5f + local.x / (local.z * tan * aspect) * 0.5f, 0.5f + local.y / (local.z * tan) * 0.5f);
        }

        static Vector3[] Corners(Bounds box)
        {
            var corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
                corners[i] = new Vector3(i % 2 == 0 ? box.min.x : box.max.x, i / 2 % 2 == 0 ? box.min.y : box.max.y,
                    i / 4 == 0 ? box.min.z : box.max.z);
            return corners;
        }

        [TestCase(3f, 3f, 1080f / 1920f)]
        [TestCase(10f, 10f, 1080f / 2400f)]
        [TestCase(6f, 6f, 1080f / 1920f)]
        [TestCase(6f, 6f, 1080f / 2400f)]
        [TestCase(10f, 4f, 1080f / 1920f)]
        [TestCase(4f, 10f, 1080f / 2400f)]
        public void FitPosition_ShouldKeepEveryCornerInsideTheMargins_WhenFramingABoard(float width, float depth, float aspect)
        {
            var box = new Bounds(new Vector3(0f, 0.25f, 0f), new Vector3(width, 0.5f, depth));

            Vector3 position = new BoardFraming().FitPosition(box, Tilt, Fov, aspect, Margins);

            float tightest = float.MaxValue;
            float top = float.MinValue, bottom = float.MaxValue;
            foreach (Vector3 corner in Corners(box))
            {
                Vector2 viewport = Viewport(corner, position, aspect);
                Assert.That(viewport.x, Is.InRange(Margins.Side - 1e-4f, 1f - Margins.Side + 1e-4f));
                Assert.That(viewport.y, Is.InRange(Margins.Bottom - 1e-4f, 1f - Margins.Top + 1e-4f));
                tightest = Mathf.Min(tightest, viewport.x - Margins.Side, 1f - Margins.Side - viewport.x,
                    viewport.y - Margins.Bottom, 1f - Margins.Top - viewport.y);
                top = Mathf.Max(top, viewport.y);
                bottom = Mathf.Min(bottom, viewport.y);
            }

            float topSlack = 1f - Margins.Top - top;
            float bottomSlack = bottom - Margins.Bottom;
            Assert.That(topSlack, Is.EqualTo(bottomSlack).Within(0.01f), "The board should sit in the middle of the band.");

            Assert.That(tightest, Is.EqualTo(0f).Within(1e-3f), "The camera should be as close as the margins allow.");
        }
    }
}
