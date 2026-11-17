using ColorBlockJam.Game.Configs;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// Where a perspective camera with a fixed rotation and field of view must stand so a box fits on screen
    /// inside the margins, centred in the free band. For each box corner it solves the smallest distance
    /// along the view axis that keeps the corner in the band and keeps the largest; a few passes then slide
    /// the aim point along the camera's up axis until the box sits in the middle of the band (the tilt makes
    /// the far edge look smaller, so aiming at the box centre alone leaves it off-centre).
    /// </summary>
    public sealed class BoardFraming
    {
        const int CentringPasses = 4;

        public Vector3 FitPosition(Bounds box, Quaternion rotation, float verticalFovDegrees, float aspect,
            FramingMargins margins)
        {
            float tanVertical = Mathf.Tan(verticalFovDegrees * 0.5f * Mathf.Deg2Rad);
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;
            // Middle of the free band in normalised device coordinates (-1 bottom, 1 top).
            float bandMiddle = margins.Bottom - margins.Top;

            float shift = 0f;
            Vector3 position = Fit(box, box.center, rotation, tanVertical, aspect, margins);
            for (int pass = 0; pass < CentringPasses; pass++)
            {
                float lowest = float.MaxValue, highest = float.MinValue;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 offset = Corner(box, i) - position;
                    float ndc = Vector3.Dot(offset, up) / (Vector3.Dot(offset, forward) * tanVertical);
                    lowest = Mathf.Min(lowest, ndc);
                    highest = Mathf.Max(highest, ndc);
                }

                // Moving the aim point up moves the box down on screen, by about ndc * tan * distance.
                float distance = Vector3.Dot(box.center - position, forward);
                shift += ((lowest + highest) * 0.5f - bandMiddle) * tanVertical * distance;
                position = Fit(box, box.center + up * shift, rotation, tanVertical, aspect, margins);
            }

            return position;
        }

        Vector3 Fit(Bounds box, Vector3 aim, Quaternion rotation, float tanVertical, float aspect,
            FramingMargins margins)
        {
            float tanHorizontal = tanVertical * aspect;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;

            // Viewport [0, 1] maps to [-tan, tan]; a margin m keeps a point within (1 - 2m) of that range.
            float sideLimit = tanHorizontal * (1f - 2f * margins.Side);
            float topLimit = tanVertical * (1f - 2f * margins.Top);
            float bottomLimit = tanVertical * (1f - 2f * margins.Bottom);

            float distance = 0f;
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = Corner(box, i) - aim;
                float x = Vector3.Dot(offset, right);
                float y = Vector3.Dot(offset, up);
                float depth = Vector3.Dot(offset, forward);

                distance = Mathf.Max(distance, Mathf.Abs(x) / sideLimit - depth);
                distance = Mathf.Max(distance, (y >= 0f ? y / topLimit : -y / bottomLimit) - depth);
            }

            return aim - forward * distance;
        }

        Vector3 Corner(Bounds box, int index)
        {
            Vector3 min = box.min, max = box.max;
            return new Vector3((index & 1) == 0 ? min.x : max.x, (index & 2) == 0 ? min.y : max.y,
                (index & 4) == 0 ? min.z : max.z);
        }
    }
}
