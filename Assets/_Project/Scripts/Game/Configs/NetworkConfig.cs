using ColorBlockJam.Core.Network;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>The online requirement's tunables (network.md, DA-25).</summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Network Config", fileName = "NetworkConfig")]
    public sealed class NetworkConfig : ScriptableObject
    {
        [Tooltip("Until the game has its own server, a public endpoint that answers 204 with a Date header (DA-25).")]
        [SerializeField]
        ConnectivitySettings _connectivity = new("https://www.google.com/generate_204", 5f, 10f, 3f);

        public ConnectivitySettings Connectivity => _connectivity;
    }
}
