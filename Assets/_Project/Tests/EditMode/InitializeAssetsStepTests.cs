using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ColorBlockJam.Game.Boot;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine.TestTools;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class InitializeAssetsStepTests
    {
        [UnityTest]
        public IEnumerator ExecuteAsync_ShouldInitializeTheSource_WhenRun() => UniTask.ToCoroutine(async () =>
        {
            var source = new FakeAssetSource();

            await new InitializeAssetsStep(source).ExecuteAsync(CancellationToken.None);

            Assert.That(source.Initializations, Is.EqualTo(1));
        });
    }
}
