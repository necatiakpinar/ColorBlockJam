using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Navigation;
using VContainer.Unity;

namespace ColorBlockJam.Game.Splash
{
    /// <summary>
    /// Orchestrates the splash: presentation and routing only. All real work lives in the boot steps.
    /// </summary>
    public sealed class SplashFlow : IAsyncStartable
    {
        readonly BootSequence _bootSequence;
        readonly SplashView _view;
        readonly SplashConfig _config;
        readonly INavigationService _navigation;

        public SplashFlow(BootSequence bootSequence, SplashView view, SplashConfig config, INavigationService navigation)
        {
            _bootSequence = bootSequence;
            _view = view;
            _config = config;
            _navigation = navigation;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _view.PlayIntro();

            UniTask minimumDisplay = UniTask.Delay(
                TimeSpan.FromSeconds(_config.MinimumDisplaySeconds),
                DelayType.Realtime,
                cancellationToken: cancellation);

            await _bootSequence.RunAsync(Progress.Create<float>(_view.ShowProgress), cancellation);
            await minimumDisplay;
            await _view.WaitForPresentationAsync(cancellation);

            await _navigation.GoToHomeAsync();
        }
    }
}
