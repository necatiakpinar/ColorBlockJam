using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Lives;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    /// <summary>
    /// The lives of one test on in-memory saves and a hand-moved trusted clock: the default
    /// <see cref="LivesConfig"/> (5 lives, 30 minutes each, refill 100 coins), a loaded wallet and lives.
    /// </summary>
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
