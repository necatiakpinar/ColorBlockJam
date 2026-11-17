using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine.TestTools;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class BootSequenceTests
    {
        [UnityTest]
        public IEnumerator RunAsync_ShouldExecuteStepsInRegistrationOrder_WhenRun() => UniTask.ToCoroutine(async () =>
        {
            var log = new List<string>();
            var sequence = new BootSequence(new IBootStep[]
            {
                new FakeBootStep("a", 1f, log),
                new FakeBootStep("b", 1f, log),
                new FakeBootStep("c", 1f, log),
            });

            await sequence.RunAsync(new RecordingProgress(), CancellationToken.None);

            Assert.That(log, Is.EqualTo(new[] { "a", "b", "c" }));
        });

        [UnityTest]
        public IEnumerator RunAsync_ShouldReportWeightedProgress_WhenStepsHaveDifferentWeights() => UniTask.ToCoroutine(async () =>
        {
            var log = new List<string>();
            var progress = new RecordingProgress();
            var sequence = new BootSequence(new IBootStep[]
            {
                new FakeBootStep("light", 1f, log),
                new FakeBootStep("heavy", 3f, log),
            });

            await sequence.RunAsync(progress, CancellationToken.None);

            Assert.That(progress.Reports, Is.EqualTo(new[] { 0f, 0.25f, 1f }).Within(0.0001f));
        });

        [UnityTest]
        public IEnumerator RunAsync_ShouldReportComplete_WhenThereAreNoSteps() => UniTask.ToCoroutine(async () =>
        {
            var progress = new RecordingProgress();
            var sequence = new BootSequence(Array.Empty<IBootStep>());

            await sequence.RunAsync(progress, CancellationToken.None);

            Assert.That(progress.Reports, Is.EqualTo(new[] { 0f, 1f }));
        });

        [UnityTest]
        public IEnumerator RunAsync_ShouldStopBeforeNextStep_WhenCancelled() => UniTask.ToCoroutine(async () =>
        {
            var log = new List<string>();
            var gate = new UniTaskCompletionSource();
            var sequence = new BootSequence(new IBootStep[]
            {
                new FakeBootStep("blocking", 1f, log, gate),
                new FakeBootStep("never", 1f, log),
            });
            using var cancellation = new CancellationTokenSource();

            UniTask run = sequence.RunAsync(new RecordingProgress(), cancellation.Token);
            cancellation.Cancel();

            bool cancelled = false;
            try
            {
                await run;
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            Assert.That(cancelled, Is.True);
            Assert.That(log, Is.EqualTo(new[] { "blocking" }));
        });

        [Test]
        public void Constructor_ShouldThrow_WhenAStepHasNonPositiveWeight()
        {
            var steps = new IBootStep[] { new FakeBootStep("zero", 0f, new List<string>()) };

            Assert.Throws<ArgumentException>(() => new BootSequence(steps));
        }
    }
}
