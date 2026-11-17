using System;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    public sealed class ExitBurstView : MonoBehaviour
    {
        [SerializeField]
        ParticleSystem _particles;

        Action<ExitBurstView> _onFinished;

        internal void Play(Vector3 localPosition, Vector2Int direction, float width, Color color, float delay,
            float duration, float debrisPerCellPerSecond, float burstPerCell, Action<ExitBurstView> onFinished)
        {
            _onFinished = onFinished;
            transform.localPosition = localPosition;
            transform.localRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y));

            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _particles.main;
            main.startDelay = delay;
            main.duration = duration;
            main.startColor = color;
            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.radius = width * 0.5f;
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = debrisPerCellPerSecond * width;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt(burstPerCell * width)));
            _particles.Play(true);
        }

        internal void Add(int count, Color color)
        {
            var emit = new ParticleSystem.EmitParams { startColor = color };
            _particles.Emit(emit, count);
        }

        void OnParticleSystemStopped() => _onFinished?.Invoke(this);
    }
}
