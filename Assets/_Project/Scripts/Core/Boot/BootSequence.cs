using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Boot
{
    public sealed class BootSequence
    {
        readonly IReadOnlyList<IBootStep> _steps;
        readonly float _totalWeight;

        public BootSequence(IReadOnlyList<IBootStep> steps)
        {
            _steps = steps;

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].Weight <= 0f)
                    throw new ArgumentException(
                        $"Boot step {steps[i].GetType().Name} has a non-positive weight ({steps[i].Weight}).");
                _totalWeight += steps[i].Weight;
            }
        }

        public async UniTask RunAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            progress.Report(0f);
            float completedWeight = 0f;

            for (int i = 0; i < _steps.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _steps[i].ExecuteAsync(cancellationToken);

                completedWeight += _steps[i].Weight;
                progress.Report(completedWeight / _totalWeight);
            }

            if (_steps.Count == 0)
                progress.Report(1f);
        }
    }
}
