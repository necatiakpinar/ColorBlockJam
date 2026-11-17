using System;
using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>How the won coins fly to the HUD counter (T-51, DA-12). Distances are in canvas units (1080x1920).</summary>
    [Serializable]
    public sealed class CoinFlightSettings
    {
        [Tooltip("Seconds each coin takes to pop out of the pile.")]
        [SerializeField, Min(0f)]
        float _popDuration = 0.18f;

        [Tooltip("How far a coin bursts out of the pile before it flies.")]
        [SerializeField, Min(0f)]
        float _scatter = 70f;

        [Tooltip("Seconds a coin takes from the pile to the counter.")]
        [SerializeField, Min(0.05f)]
        float _flyDuration = 0.6f;

        [Tooltip("Seconds between one coin leaving and the next.")]
        [SerializeField, Min(0f)]
        float _stagger = 0.07f;

        [Tooltip("Ease of the sideways part of the flight; the rise or fall uses its own, so the path bends.")]
        [SerializeField]
        Ease _sidewaysEase = Ease.InOutSine;

        [SerializeField]
        Ease _upwardEase = Ease.InQuad;

        [Tooltip("A coin's scale as it reaches the counter.")]
        [SerializeField, Min(0f)]
        float _arriveScale = 0.6f;

        [Tooltip("How much the counter's coin swells when a coin lands.")]
        [SerializeField, Min(0f)]
        float _punch = 0.25f;

        [SerializeField, Min(0f)]
        float _punchDuration = 0.2f;

        [Tooltip("How far the sparkles fly out from the counter.")]
        [SerializeField, Min(0f)]
        float _sparkleDistance = 120f;

        [SerializeField, Min(0.05f)]
        float _sparkleDuration = 0.45f;

        [Tooltip("Seconds after the last coin lands before the game goes Home.")]
        [SerializeField, Min(0f)]
        float _homeDelay = 0.35f;

        public float PopDuration => _popDuration;
        public float Scatter => _scatter;
        public float FlyDuration => _flyDuration;
        public float Stagger => _stagger;
        public Ease SidewaysEase => _sidewaysEase;
        public Ease UpwardEase => _upwardEase;
        public float ArriveScale => _arriveScale;
        public float Punch => _punch;
        public float PunchDuration => _punchDuration;
        public float SparkleDistance => _sparkleDistance;
        public float SparkleDuration => _sparkleDuration;
        public float HomeDelay => _homeDelay;
    }
}
