using System;
using System.Collections.Generic;

namespace RollicCase.Tests.EditMode.Helpers
{
    /// <summary>Synchronous IProgress (System.Progress posts to a SynchronizationContext).</summary>
    public sealed class RecordingProgress : IProgress<float>
    {
        public List<float> Reports { get; } = new();

        public void Report(float value) => Reports.Add(value);
    }
}
