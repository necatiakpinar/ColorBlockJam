using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Audio;
using ColorBlockJam.Game.Boot;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Network;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Boosters;
using ColorBlockJam.Game.Gameplay.Boosters.Commands;
using ColorBlockJam.Game.Gameplay.Feedback;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Installers;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Packs;
using ColorBlockJam.Game.Lives;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Tests.EditMode
{
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

        static List<System.Type> ConfigTypes() =>
            typeof(ConfigProvider).Assembly.GetTypes()
                .Where(type => typeof(ScriptableObject).IsAssignableFrom(type) && type.Name.EndsWith("Config") && !type.IsAbstract)
                .ToList();

        ConfigProvider Configs(BoardViewConfig boardView = null)
        {
            var provider = CreateConfig<ConfigProvider>();
            var serialized = new SerializedObject(provider);
            SerializedProperty list = serialized.FindProperty("_configs");
            List<System.Type> types = ConfigTypes();
            list.arraySize = types.Count;
            for (int i = 0; i < types.Count; i++)
            {
                ScriptableObject config = boardView;
                if (types[i] != typeof(BoardViewConfig) || boardView == null)
                {
                    config = ScriptableObject.CreateInstance(types[i]);
                    _createdAssets.Add(config);
                }

                list.GetArrayElementAtIndex(i).objectReferenceValue = config;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return provider;
        }

        [Test]
        public void ConfigInstaller_ShouldRegisterEveryConfigUnderItsOwnType_WhenBuilt()
        {
            ConfigProvider configs = Configs();
            var builder = new ContainerBuilder();
            new ConfigInstaller(configs).Install(builder);
            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<ConfigProvider>(), Is.SameAs(configs));
            foreach (ScriptableObject config in configs.All)
                Assert.That(container.Resolve(config.GetType()), Is.SameAs(config), config.GetType().Name);
            Assert.That(configs.GetConfig<GameplayConfig>(), Is.SameAs(container.Resolve<GameplayConfig>()));
        }

        [Test]
        public void ShippedConfigProvider_ShouldHoldEveryConfigOnce_WhenTheGameShips()
        {
            var shipped = AssetDatabase.LoadAssetAtPath<ConfigProvider>("Assets/_Project/Configs/ConfigProvider.asset");
            var types = new List<System.Type>();
            foreach (ScriptableObject config in shipped.All)
            {
                Assert.That(config, Is.Not.Null, "Every slot is assigned.");
                types.Add(config.GetType());
            }

            Assert.That(types, Is.Unique);
            foreach (System.Type type in ConfigTypes())
                Assert.That(types, Does.Contain(type), $"{type.Name} is missing from the ConfigProvider.");
        }

        IObjectResolver BuildCoreAndSplash()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(Configs()).Install(builder);
            new SplashInstaller().Install(builder);
            return builder.Build();
        }

        [Test]
        public void CoreServicesInstaller_ShouldResolveEveryService_WhenBuilt()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(Configs()).Install(builder);
            builder.RegisterInstance<IAudioOutput>(new Helpers.RecordingAudioOutput());
            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<SignalBus>(), Is.Not.Null);
            Assert.That(container.Resolve<IGameLogger>(), Is.Not.Null);
            Assert.That(container.Resolve<IKeyValueStorage>(), Is.Not.Null);
            Assert.That(container.Resolve<IPlayerProgressService>(), Is.Not.Null);
            Assert.That(container.Resolve<IWalletService>(), Is.Not.Null);
            Assert.That(container.Resolve<ISettingsService>(), Is.Not.Null);
            Assert.That(container.Resolve<BoosterInventory>(), Is.Not.Null);
            Assert.That(container.Resolve<LivesService>(), Is.Not.Null);
            Assert.That(container.Resolve<ISceneLoader>(), Is.Not.Null);
            Assert.That(container.Resolve<INavigationService>(), Is.Not.Null);
            Assert.That(container.Resolve<UiConfig>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelPackConfig>(), Is.Not.Null);
            Assert.That(container.Resolve<ILevelSource>(), Is.SameAs(container.Resolve<LevelPackService>()));
            Assert.That(container.Resolve<IHapticService>(), Is.Not.Null);
            Assert.That(container.Resolve<IGameClock>(), Is.Not.Null);
            Assert.That(container.Resolve<IConnectivityService>(), Is.Not.Null);
            Assert.That(container.Resolve<IAudioService>(), Is.Not.Null);
            Assert.That(container.Resolve<ITrustedClock>(), Is.SameAs(container.Resolve<TrustedClock>()));
        }

        [Test]
        public void CoreServicesInstaller_ShouldShareOneSignalBus_WhenResolvedTwice()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(Configs()).Install(builder);
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
                typeof(LoadLevelsStep),
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

        IObjectResolver BuildCoreAndGameplay()
        {
            var builder = new ContainerBuilder();
            new CoreServicesInstaller(Configs()).Install(builder);
            builder.RegisterInstance<IAudioOutput>(new Helpers.RecordingAudioOutput());
            new GameplayInstaller().Install(builder);
            return builder.Build();
        }

        IObjectResolver BuildGameplay(BoardViewConfig boardViewConfig)
        {
            var builder = new ContainerBuilder();
            new ConfigInstaller(Configs(boardViewConfig)).Install(builder);
            new GameplayInstaller().Install(builder);
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
            using IObjectResolver container = BuildCoreAndGameplay();

            Assert.That(container.Resolve<LevelProvider>(), Is.Not.Null);
            Assert.That(container.Resolve<ActiveBoard>(), Is.Not.Null);
            Assert.That(container.Resolve<BlockDragController>(), Is.Not.Null);
            Assert.That(container.Resolve<GameplayHaptics>(), Is.Not.Null);
            Assert.That(container.Resolve<GameplaySounds>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelSession>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelWinService>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelTimer>(), Is.Not.Null);
            Assert.That(container.Resolve<BoosterShop>(), Is.Not.Null);
            Assert.That(container.Resolve<BoosterService>(), Is.Not.Null);
            Assert.That(container.Resolve<LevelAttemptLives>(), Is.Not.Null);
        }

        [Test]
        public void GameplayInstaller_ShouldRegisterOneCommandPerBooster_WhenBuiltOnTopOfCoreServices()
        {
            using IObjectResolver container = BuildCoreAndGameplay();

            IEnumerable<BoosterType> types = container.Resolve<IReadOnlyList<IBoosterCommand>>().Select(command => command.Type);

            Assert.That(types, Is.EquivalentTo(System.Enum.GetValues(typeof(BoosterType))));
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
