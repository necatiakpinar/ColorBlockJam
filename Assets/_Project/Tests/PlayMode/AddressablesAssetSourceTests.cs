using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ColorBlockJam.Core.Assets;
using UnityEngine;
using UnityEngine.TestTools;

namespace ColorBlockJam.Tests.PlayMode
{
    public sealed class AddressablesAssetSourceTests
    {
        const string MissingKey = "ColorBlockJam/NoSuchAsset";

        [UnityTest]
        public IEnumerator InitializeAsync_ShouldFinish_WhenTheCatalogIsLocal() => UniTask.ToCoroutine(async () =>
        {
            var source = new AddressablesAssetSource();

            await source.InitializeAsync(CancellationToken.None).Timeout(TimeSpan.FromSeconds(10));

            LogAssert.NoUnexpectedReceived();
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldThrow_WhenTheKeyIsUnknown() => UniTask.ToCoroutine(async () =>
        {
            var source = new AddressablesAssetSource();
            await source.InitializeAsync(CancellationToken.None);
            LogAssert.Expect(LogType.Error, new Regex("InvalidKeyException"));
            string failure = null;

            try
            {
                await source.LoadAsync<TextAsset>(MissingKey);
            }
            catch (Exception exception)
            {
                failure = exception.ToString();
            }

            Assert.That(failure, Does.Contain(MissingKey));
        });
    }
}
