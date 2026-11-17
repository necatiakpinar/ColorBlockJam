using ColorBlockJam.Core.Signals;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Core.Lifecycle
{
    public sealed class ApplicationPauseReporter : MonoBehaviour
    {
        SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus) => _signalBus = signalBus;

        void OnApplicationPause(bool pauseStatus) => _signalBus?.Fire(new ApplicationPausedSignal(pauseStatus));
    }
}
