using System;
using UnityEngine;

namespace ColorBlockJam.Core.Network
{
    [Serializable]
    public struct ConnectivitySettings
    {
        [Tooltip("Any URL that answers quickly with a 2xx status and an HTTP Date header.")]
        [SerializeField]
        string _probeUrl;

        [Tooltip("Seconds a probe may take before it counts as offline.")]
        [SerializeField, Min(1f)]
        float _probeTimeout;

        [Tooltip("Seconds between probes while online.")]
        [SerializeField, Min(1f)]
        float _onlineInterval;

        [Tooltip("Seconds between probes while offline, so the game notices the connection coming back.")]
        [SerializeField, Min(1f)]
        float _offlineInterval;

        public ConnectivitySettings(string probeUrl, float probeTimeout, float onlineInterval, float offlineInterval)
        {
            _probeUrl = probeUrl;
            _probeTimeout = probeTimeout;
            _onlineInterval = onlineInterval;
            _offlineInterval = offlineInterval;
        }

        public string ProbeUrl => _probeUrl;
        public float ProbeTimeout => _probeTimeout;
        public float OnlineInterval => _onlineInterval;
        public float OfflineInterval => _offlineInterval;
    }
}
