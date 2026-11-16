using UnityEngine;

namespace RollicCase.Core.Logging
{
    public sealed class UnityGameLogger : IGameLogger
    {
        const string Prefix = "[Game] ";

        public void Log(string message) => Debug.Log(Prefix + message);

        public void LogWarning(string message) => Debug.LogWarning(Prefix + message);

        public void LogError(string message) => Debug.LogError(Prefix + message);
    }
}
