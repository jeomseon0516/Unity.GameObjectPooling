using System;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Demonstrates a low-level IGameObjectPool decorator that adds diagnostics while
    /// preserving the underlying storage behavior.
    /// 내부 저장소 동작은 유지하면서 진단 기능을 추가하는 저수준 IGameObjectPool
    /// Decorator 예제입니다.
    /// </summary>
    public sealed class CountingGameObjectPool : IGameObjectPool
    {
        private readonly IGameObjectPool _innerPool;

        /// <summary>
        /// Gets the total number of successful spawn operations.
        /// 성공한 전체 생성 횟수를 가져옵니다.
        /// </summary>
        public int SpawnCount { get; private set; }

        /// <summary>
        /// Gets the total number of successful despawn operations.
        /// 성공한 전체 반환 횟수를 가져옵니다.
        /// </summary>
        public int DespawnCount { get; private set; }

        /// <summary>
        /// Creates a counting decorator over another pool implementation.
        /// 다른 풀 구현을 감싸는 Counting Decorator를 생성합니다.
        /// </summary>
        public CountingGameObjectPool(IGameObjectPool innerPool)
        {
            _innerPool = innerPool ?? throw new ArgumentNullException(nameof(innerPool));
        }

        /// <inheritdoc />
        public GameObject Get()
        {
            GameObject instance = _innerPool.Get();
            SpawnCount++;
            return instance;
        }

        /// <inheritdoc />
        public GameObject Get(in PoolSpawnOptions options)
        {
            GameObject instance = _innerPool.Get(options);
            SpawnCount++;
            return instance;
        }

        /// <inheritdoc />
        public T Get<T>() where T : Component
        {
            T instance = _innerPool.Get<T>();
            SpawnCount++;
            return instance;
        }

        /// <inheritdoc />
        public T Get<T>(in PoolSpawnOptions options) where T : Component
        {
            T instance = _innerPool.Get<T>(options);
            SpawnCount++;
            return instance;
        }

        /// <inheritdoc />
        public void Release(GameObject instance)
        {
            _innerPool.Release(instance);
            DespawnCount++;
        }

        /// <inheritdoc />
        public void Release<T>(T component) where T : Component
        {
            _innerPool.Release(component);
            DespawnCount++;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _innerPool.Dispose();
        }
    }
}
