using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Lives;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class LivesRig : IDisposable
    {
        public LivesRig(SignalBus bus, IWalletService wallet = null)
        {
            Config = ScriptableObject.CreateInstance<LivesConfig>();
            Storage = new InMemoryKeyValueStorage();
            Clock = new FakeTrustedClock();
            if (wallet == null)
            {
                var own = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), bus);
                own.Load();
                wallet = own;
            }

            Wallet = wallet;
            Lives = new LivesService(Storage, new RecordingLogger(), Config, Clock, Wallet, bus);
            Lives.Load();
        }

        public LivesConfig Config { get; }
        public InMemoryKeyValueStorage Storage { get; }
        public FakeTrustedClock Clock { get; }
        public IWalletService Wallet { get; }
        public LivesService Lives { get; }

        public void LoseAll()
        {
            while (Lives.TryLoseOne())
            {
            }
        }

        public void Dispose() => Object.DestroyImmediate(Config);
    }
}
