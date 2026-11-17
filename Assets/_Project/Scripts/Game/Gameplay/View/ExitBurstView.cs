using System;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// The spray of small cubes out of a door while a block slides through it. Pooled; the particle system
    /// (authored in <c>BlockExitBurst.prefab</c>, stop action Callback) hands the view back when its last
    /// particle is gone.
    /// </summary>
    public sealed class ExitBurstView : MonoBehaviour
    {
        [SerializeField]
        ParticleSystem _particles;

        Action<ExitBurstView> _onFinished;

        /// <param name="localPosition">Middle of the door opening the block passes, in board cells.</param>
        /// <param name="width">Width of the block across the door, in cells.</param>
        /// <param name="delay">How long the block takes to reach the door: the spray starts then.</param>
        /// <param name="duration">How long the block takes to pass: the spray lasts as long.</param>
        internal void Play(Vector3 localPosition, Vector2Int direction, float width, float height, Color color,
            float delay, float duration, float debrisPerCellPerSecond, Action<ExitBurstView> onFinished)
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
            shape.scale = new Vector3(width, height, shape.scale.z);
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = debrisPerCellPerSecond * width;
            _particles.Play(true);
        }

        // Sent by the particle system (stop action Callback) once every particle has died.
        void OnParticleSystemStopped() => _onFinished?.Invoke(this);
    }
}
