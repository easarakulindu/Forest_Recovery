using NUnit.Framework;
using UnityEngine;

namespace ForestRecovery.Tests
{
    public sealed class RestorationStateTests
    {
        [Test]
        public void PlantingWithoutASeedDoesNotRestore()
        {
            var state = new RestorationState();
            Assert.That(state.Plant(), Is.False);
            Assert.That(state.Seeds, Is.Zero);
            Assert.That(state.TreesPlanted, Is.Zero);
            Assert.That(state.Health, Is.EqualTo(40));
        }

        [Test]
        public void PlantingConsumesOneSeedAndCompletionCapsAtOneHundred()
        {
            var state = new RestorationState();
            for (int i = 0; i < 3; i++)
            {
                Assert.That(state.CollectSeed(), Is.True);
                Assert.That(state.Health, Is.EqualTo(40 + i * 20));
                Assert.That(state.Plant(), Is.True);
                Assert.That(state.Health, Is.EqualTo(60 + i * 20));
            }
            Assert.That(state.Seeds, Is.Zero);
            Assert.That(state.Complete, Is.True);
            Assert.That(state.CollectSeed(), Is.False);
            Assert.That(state.Plant(), Is.False);
            Assert.That(state.Health, Is.EqualTo(100));
        }

        [Test]
        public void ACollectibleCannotBeUsedTwice()
        {
            var go = new GameObject("Test seed");
            try
            {
                var item = go.AddComponent<ForestInteractable>();
                item.action = ForestAction.Seed;
                var state = new RestorationState();
                Assert.That(item.Apply(state), Is.True);
                Assert.That(item.Apply(state), Is.False);
                Assert.That(state.Seeds, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
