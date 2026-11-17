using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Boosters;
using ColorBlockJam.Game.Gameplay.Session;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    /// <summary>
    /// The booster services of one gameplay scene on a fake clock and in-memory saves: the default
    /// <see cref="BoosterConfig"/> (one of each, freeze 10 s), a loaded inventory and wallet, the level timer, the
    /// shop and the service. Dispose it in TearDown.
    /// </summary>
    public sealed class BoosterRig : IDisposable
    {
        public BoosterRig(ActiveBoard board, LevelSession session, SignalBus bus)
        {
            Config = ScriptableObject.CreateInstance<BoosterConfig>();
            Clock = new FakeGameClock();
            Storage = new InMemoryKeyValueStorage();
            Inventory = new BoosterInventory(Storage, new RecordingLogger(), Config, bus);
            Inventory.Load();
            Wallet = new WalletService(Storage, new RecordingLogger(), bus);
            Wallet.Load();
            Timer = new LevelTimer(session, Clock, bus);
            Shop = new BoosterShop(Wallet, Inventory, Config);
            Service = new BoosterService(board, session, Timer, Inventory, Config, bus);
        }

        public BoosterConfig Config { get; }
        public FakeGameClock Clock { get; }
        public InMemoryKeyValueStorage Storage { get; }
        public BoosterInventory Inventory { get; }
        public WalletService Wallet { get; }
        public LevelTimer Timer { get; }
        public BoosterShop Shop { get; }
        public BoosterService Service { get; }

        /// <summary>Spends every booster of <paramref name="type"/> the player owns.</summary>
        public void UseUp(BoosterType type)
        {
            while (Inventory.TryUse(type))
            {
            }
        }

        public void Dispose()
        {
            Service.Dispose();
            Timer.Dispose();
            Object.DestroyImmediate(Config);
        }
    }
}
