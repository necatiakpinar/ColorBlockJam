using System.Collections.Generic;
using RollicCase.Core.Logging;

namespace RollicCase.Tests.EditMode.Helpers
{
    public sealed class RecordingLogger : IGameLogger
    {
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();

        public void Log(string message) { }

        public void LogWarning(string message) => Warnings.Add(message);

        public void LogError(string message) => Errors.Add(message);
    }
}
