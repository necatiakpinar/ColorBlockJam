using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RollicCase.Core.Boot;
using RollicCase.Core.Scenes;
using RollicCase.Game.Configs;
using VContainer.Unity;

namespace RollicCase.Game.Splash
{
    /// <summary>
    /// Orchestrates the splash: presentation and routing only. All real work lives in the boot steps.
    /// </summary>
    public sealed class SplashFlow : IAsyncStartable
    {
        readonly BootSequence _bootSequence;
        readonly SplashView _view;
        readonly SplashConfig _config;
        readonly ISceneLoader _sceneLoader;

        public SplashFlow(BootSequence bootSequence, SplashView view, SplashConfig config, ISceneLoader sceneLoader)
        {
            _bootSequence = bootSequence;
            _view = view;
            _config = config;
            _sceneLoader = sceneLoader;
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

            // Loading Gameplay destroys this scope and cancels its token, so the load itself must not
            // listen to that token or it would report its own success as a cancellation.
            await _sceneLoader.LoadAsync(SceneId.Gameplay, CancellationToken.None);
        }
    }
}
