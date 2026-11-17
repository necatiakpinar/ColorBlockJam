using ColorBlockJam.Game.Configs;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    public sealed class BoardFraming
    {
        const int CentringPasses = 4;

        public Vector3 FitPosition(Bounds box, Quaternion rotation, float verticalFovDegrees, float aspect,
            FramingMargins margins)
        {
            float tanVertical = Mathf.Tan(verticalFovDegrees * 0.5f * Mathf.Deg2Rad);
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;
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
