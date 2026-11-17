using System;
using System.Collections.Generic;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class RecordingProgress : IProgress<float>
    {
        public List<float> Reports { get; } = new();

        public void Report(float value) => Reports.Add(value);
    }
}
