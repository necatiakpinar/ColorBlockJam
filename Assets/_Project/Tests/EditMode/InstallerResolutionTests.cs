using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boot;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Feedback;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Installers;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>Builds the real installers into a plain container: catches missing or broken registrations.</summary>
    public sealed class InstallerResolutionTests
    {
        readonly List<Object> _createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Object asset in _createdAssets)
                Object.DestroyImmediate(asset);
            _createdAssets.Clear();
        }

        T CreateConfig<T>() where T : ScriptableObject
        {
            var config = ScriptableObject.CreateInstance<T>();
            _createdAssets.Add(config);
            return config;
        }

        IObjectResolver BuildCoreAndSplash()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(CreateConfig<ApplicationConfig>(), CreateConfig<UiConfig>(), CreateConfig<LevelCatalog>(),
                CreateConfig<NetworkConfig>(), CreateConfig<BoosterConfig>()).Install(builder);
            new SplashInstaller(CreateConfig<SplashConfig>()).Install(builder);
            return builder.Build();
        }

        [Test]
        public void CoreServicesInstaller_ShouldResolveEveryService_WhenBuilt()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(CreateConfig<ApplicationConfig>(), CreateConfig<UiConfig>(), CreateConfig<LevelCatalog>(),
                CreateConfig<NetworkConfig>(), CreateConfig<BoosterConfig>()).Install(builder);
            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<SignalBus>(), Is.Not.Null);
            Assert.That(container.Resolve<IGameLogger>(), Is.Not.Null);
            Assert.That(container.Resolve<IKeyValueStorage>(), Is.Not.Null);
            Assert.That(container.Resolve<IPlayerProgressService>(), Is.Not.Null);
            Assert.That(container.Resolve<IWalletService>(), Is.Not.Null);
            Assert.That(container.Resolve<ISettingsService>(), Is.Not.Null);
            Assert.That(container.Resolve<BoosterInventory>(), Is.Not.Null);
            Assert.That(container.Resolve<ISceneLoader>(), Is.Not.Null);
            Assert.That(container.Resolve<INavigationService>(), Is.Not.Null);
            Assert.That(container.Resolve<UiConfig>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelCatalog>(), Is.Not.Null);
            Assert.That(container.Resolve<IHapticService>(), Is.Not.Null);
            Assert.That(container.Resolve<IGameClock>(), Is.Not.Null);
            Assert.That(container.Resolve<IConnectivityService>(), Is.Not.Null);
            Assert.That(container.Resolve<ITrustedClock>(), Is.SameAs(container.Resolve<TrustedClock>()));
        }

        [Test]
        public void CoreServicesInstaller_ShouldShareOneSignalBus_WhenResolvedTwice()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(CreateConfig<ApplicationConfig>(), CreateConfig<UiConfig>(), CreateConfig<LevelCatalog>(),
                CreateConfig<NetworkConfig>(), CreateConfig<BoosterConfig>()).Install(builder);
            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<SignalBus>(), Is.SameAs(container.Resolve<SignalBus>()));
        }

        [Test]
        public void SplashInstaller_ShouldRegisterBootStepsInExecutionOrder_WhenBuilt()
        {
            using IObjectResolver container = BuildCoreAndSplash();

            IReadOnlyList<IBootStep> steps = container.Resolve<IReadOnlyList<IBootStep>>();

            Assert.That(steps.Select(step => step.GetType()), Is.EqualTo(new[]
            {
                typeof(ApplyApplicationSettingsStep),
                typeof(LoadPlayerDataStep),
            }));
        }

        [Test]
        public void HomeInstaller_ShouldResolvePopupService_WhenBuilt()
        {
            var builder = new ContainerBuilder();
            new HomeInstaller().Install(builder);
            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<IPopupService>(), Is.Not.Null);
        }

        IObjectResolver BuildGameplay(BoardViewConfig boardViewConfig)
        {
            var builder = new ContainerBuilder();
            new GameplayInstaller(CreateConfig<GameplayConfig>(), boardViewConfig, CreateConfig<LevelRewardConfig>()).Install(builder);
            return builder.Build();
        }

        [Test]
        public void GameplayInstaller_ShouldResolveBoardSpaceWithConfiguredCellSize_WhenBuilt()
        {
            var boardViewConfig = CreateConfig<BoardViewConfig>();
            var serialized = new SerializedObject(boardViewConfig);
            serialized.FindProperty("_cellSize").floatValue = 2.5f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            using IObjectResolver container = BuildGameplay(boardViewConfig);

            Assert.That(container.Resolve<BoardSpace>().CellSize, Is.EqualTo(2.5f));
        }

        [Test]
        public void GameplayInstaller_ShouldResolveBoardServices_WhenBuiltOnTopOfCoreServices()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(CreateConfig<ApplicationConfig>(), CreateConfig<UiConfig>(), CreateConfig<LevelCatalog>(),
                CreateConfig<NetworkConfig>(), CreateConfig<BoosterConfig>())
                .Install(builder);
            new GameplayInstaller(CreateConfig<GameplayConfig>(), CreateConfig<BoardViewConfig>(), CreateConfig<LevelRewardConfig>()).Install(builder);
            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<LevelProvider>(), Is.Not.Null);
            Assert.That(container.Resolve<ActiveBoard>(), Is.Not.Null);
            Assert.That(container.Resolve<BlockDragController>(), Is.Not.Null);
            Assert.That(container.Resolve<GameplayHaptics>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelSession>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelProgression>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelTimer>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelRewardService>(), Is.Not.Null);
        }

        [Test]
        public void GameplayInstaller_ShouldResolvePopupService_WhenBuilt()
        {
            using IObjectResolver container = BuildGameplay(CreateConfig<BoardViewConfig>());

            Assert.That(container.Resolve<IPopupService>(), Is.Not.Null);
        }

        [Test]
        public void SplashInstaller_ShouldResolveBootSequence_WhenBuilt()
        {
            using IObjectResolver container = BuildCoreAndSplash();

            Assert.That(container.Resolve<BootSequence>(), Is.Not.Null);
        }
    }
}
