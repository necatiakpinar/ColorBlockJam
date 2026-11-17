using ColorBlockJam.Core.Signals;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Core.Lifecycle
{
    /// <summary>
    /// Announces the app going to the background and coming back as <see cref="ApplicationPausedSignal"/>.
    /// Lives on the Root scope object so it outlives every scene. <c>OnApplicationPause</c> is Unity's event
    /// for it; nothing runs per frame.
    /// </summary>
    public sealed class ApplicationPauseReporter : MonoBehaviour
    {
        SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus) => _signalBus = signalBus;

        // Unity also reports "not paused" once at startup, which can come before the scope injects this.
        void OnApplicationPause(bool pauseStatus) => _signalBus?.Fire(new ApplicationPausedSignal(pauseStatus));
    }
}
