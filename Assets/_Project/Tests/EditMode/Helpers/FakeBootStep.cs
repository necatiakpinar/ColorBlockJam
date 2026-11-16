using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RollicCase.Core.Boot;

namespace RollicCase.Tests.EditMode.Helpers
{
    public sealed class FakeBootStep : IBootStep
    {
        readonly string _name;
        readonly List<string> _executionLog;
        readonly UniTaskCompletionSource _gate;

        public FakeBootStep(string name, float weight, List<string> executionLog, UniTaskCompletionSource gate = null)
        {
            _name = name;
            Weight = weight;
            _executionLog = executionLog;
            _gate = gate;
        }

        public float Weight { get; }

        public async UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            _executionLog.Add(_name);
            if (_gate != null)
                await _gate.Task.AttachExternalCancellation(cancellationToken);
        }
    }
}
