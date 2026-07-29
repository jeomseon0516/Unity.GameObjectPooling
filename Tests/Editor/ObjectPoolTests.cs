using Jeomseon.ObjectPool;
using NUnit.Framework;
using UnityEngine;

namespace Jeomseon.Tests
{
    public sealed class ObjectPoolTests
    {
        private sealed class PooledComponent : MonoBehaviour, IPooledObject
        {
            [PoolInit(7)]
            public int Value;

            public int PopCount { get; private set; }
            public int ReturnCount { get; private set; }

            public void OnPopFromPool() => PopCount++;
            public void OnReturnToPool() => ReturnCount++;
        }

        [TearDown]
        public void TearDown()
        {
            GenericObjectPool<PooledComponent>.ReleaseAllObject();
        }

        [Test]
        public void ReturnAndPop_ResetStateAndInvokeLifecycleCallbacks()
        {
            GameObject gameObject = new("Pooled component");
            PooledComponent component = gameObject.AddComponent<PooledComponent>();
            component.Value = 100;

            GenericObjectPool<PooledComponent>.Return(component);

            Assert.That(component.Value, Is.EqualTo(7));
            Assert.That(component.ReturnCount, Is.EqualTo(1));
            Assert.That(gameObject.activeSelf, Is.False);

            PooledComponent popped = GenericObjectPool<PooledComponent>.Pop();

            Assert.That(popped, Is.SameAs(component));
            Assert.That(popped.PopCount, Is.EqualTo(1));
            Assert.That(gameObject.activeSelf, Is.True);

            Object.DestroyImmediate(gameObject);
        }
    }
}
