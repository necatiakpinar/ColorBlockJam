using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Configs;
using UnityEngine;

namespace ColorBlockJam.Game.Boot
{
    public sealed class ApplyApplicationSettingsStep : IBootStep
    {
        readonly ApplicationConfig _config;

        public ApplyApplicationSettingsStep(ApplicationConfig config)
        {
            _config = config;
        }

        public float Weight => 1f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            Application.targetFrameRate = _config.TargetFrameRate;
            return UniTask.CompletedTask;
        }
    }
}
