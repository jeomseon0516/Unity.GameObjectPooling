using System;
using System.Collections.Generic;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using Jeomseon.Unity.GameObjectPooling.Lifecycle;
using Jeomseon.Unity.GameObjectPooling.Reset;
using UnityEngine;
using UnityEngine.Pool;
using UnityObject = UnityEngine.Object;

namespace Jeomseon.Unity.GameObjectPooling.Pools
{
    /// <summary>
    /// Default GameObject pool implementation backed by Unity's ObjectPool. This class owns
    /// Unity-specific activation, transform, callback and destruction behavior; storage and
    /// inactive-capacity management remain delegated to Unity.
    /// Unity ObjectPool 기반의 기본 GameObject 풀 구현입니다. 이 클래스는 Unity 전용 활성화,
    /// Transform, 콜백 및 파괴 동작을 소유하며 저장소와 비활성 용량 관리는 Unity에 위임합니다.
    /// </summary>
    public sealed class UnityGameObjectPool :
        IGameObjectPool,
        IClearablePool,
        IPrewarmablePool,
        IPoolDiagnostics
    {
        private readonly IObjectPool<GameObject> _storage;
        private readonly UnityGameObjectPoolConfiguration _configuration;
        private readonly HashSet<GameObject> _activeInstances = new();
        private readonly GameObject _root;
        private bool _disposed;
        private int _createdCount;
        private int _destroyedCount;
        private int _releasedCount;
        private int _invalidReleaseCount;
        private int _capacityDiscardedCount;

        /// <summary>
        /// Gets the number of currently active instances.
        /// 현재 활성 상태인 인스턴스 개수를 가져옵니다.
        /// </summary>
        public int CountActive => _activeInstances.Count;

        /// <summary>
        /// Gets the number of instances retained by Unity's underlying pool.
        /// Unity 내부 풀이 보관 중인 인스턴스 개수를 가져옵니다.
        /// </summary>
        public int CountInactive => _storage.CountInactive;

        /// <inheritdoc />
        public PoolStatistics Statistics
        {
            get
            {
                RemoveDestroyedActiveInstances();
                return new PoolStatistics(
                    _activeInstances.Count,
                    _storage.CountInactive,
                    _createdCount,
                    _destroyedCount,
                    _releasedCount,
                    _invalidReleaseCount,
                    _capacityDiscardedCount);
            }
        }

        /// <summary>
        /// Creates the default Unity-backed implementation for the supplied configuration.
        /// 지정된 Configuration에 대한 기본 Unity 기반 구현을 생성합니다.
        /// </summary>
        public UnityGameObjectPool(
            UnityGameObjectPoolConfiguration configuration,
            Transform runtimeRoot = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configuration = configuration;
            _root = new GameObject($"{configuration.Name} Pool");
            _root.SetActive(false);
            _root.transform.SetParent(runtimeRoot, false);

            _storage = new ObjectPool<GameObject>(
                CreateInstance,
                actionOnGet: null,
                actionOnRelease: null,
                DestroyInstance,
                configuration.CollectionCheck,
                configuration.DefaultCapacity,
                configuration.MaxInactiveCount);

            Prewarm(configuration.PrewarmCount);
        }

        /// <inheritdoc />
        public GameObject Get()
        {
            return Get(default);
        }

        /// <inheritdoc />
        public GameObject Get(in PoolSpawnOptions options)
        {
            ThrowIfDisposed();

            GameObject instance;
            while ((instance = _storage.Get()) == null)
            {
                _destroyedCount++;
                DestroyedInstancePolicy policy = _configuration.DestroyedInstancePolicy;
                if (policy == DestroyedInstancePolicy.Throw)
                {
                    throw new InvalidOperationException(
                        "A pooled instance was destroyed outside its owning pool.");
                }

                if (policy == DestroyedInstancePolicy.WarnAndReplace)
                {
                    Debug.LogWarning(
                        $"[{nameof(UnityGameObjectPool)}] A destroyed pooled instance was " +
                        $"replaced for {_configuration.Name}.");
                }
            }

            PooledGameObjectState state = GetState(instance);
            if (!state.IsReleased)
            {
                throw new InvalidOperationException("The pooled instance is already active.");
            }

            ApplySpawnOptions(instance.transform, options);
            state.IsReleased = false;
            _activeInstances.Add(instance);
            instance.SetActive(true);

            foreach (IPoolGetHandler handler in state.GetHandlers)
            {
                handler.OnGetFromPool();
            }

            return instance;
        }

        /// <inheritdoc />
        public T Get<T>() where T : Component
        {
            return Get<T>(default);
        }

        /// <inheritdoc />
        public T Get<T>(in PoolSpawnOptions options) where T : Component
        {
            if (!_configuration.Prefab.TryGetComponent(out T _))
            {
                throw new InvalidOperationException(
                    $"The pooled prefab does not contain {typeof(T).FullName}.");
            }

            GameObject instance = Get(options);
            return instance.GetComponent<T>();
        }

        /// <inheritdoc />
        public void Release<T>(T component) where T : Component
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            Release(component.gameObject);
        }

        /// <inheritdoc />
        public void Release(GameObject instance)
        {
            ThrowIfDisposed();
            if (ReferenceEquals(instance, null))
            {
                _invalidReleaseCount++;
                throw new ArgumentNullException(nameof(instance));
            }

            if (instance == null)
            {
                HandleDestroyedRelease(instance);
                return;
            }

            PooledGameObjectState state = GetState(instance);
            if (state.Owner != this)
            {
                _invalidReleaseCount++;
                throw new InvalidOperationException(
                    "The instance belongs to a different pool.");
            }

            if (state.IsReleased || !_activeInstances.Remove(instance))
            {
                _invalidReleaseCount++;
                throw new InvalidOperationException(
                    "The instance has already been released.");
            }

            state.IsReleased = true;
            try
            {
                foreach (IPoolReleaseHandler handler in state.ReleaseHandlers)
                {
                    handler.OnReleaseToPool();
                }

                if (_configuration.ResetAttributedMembers)
                {
                    PoolReleaseResetter.Reset(instance);
                }

                instance.SetActive(false);
                instance.transform.SetParent(_root.transform, false);
                bool discardsForCapacity =
                    _storage.CountInactive >= _configuration.MaxInactiveCount;
                _storage.Release(instance);
                if (discardsForCapacity)
                {
                    _capacityDiscardedCount++;
                }

                _releasedCount++;
            }
            catch
            {
                state.IsReleased = false;
                _activeInstances.Add(instance);
                throw;
            }
        }

        /// <summary>
        /// Creates and stores inactive instances without invoking gameplay callbacks.
        /// 게임플레이 콜백을 호출하지 않고 비활성 인스턴스를 생성하여 보관합니다.
        /// </summary>
        public void Prewarm(int count)
        {
            ThrowIfDisposed();
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            int targetCount = Math.Min(
                _configuration.MaxInactiveCount,
                _storage.CountInactive + count);

            if (_storage.CountInactive >= targetCount) return;

            var instances = new List<GameObject>(targetCount);
            for (int i = 0; i < targetCount; i++)
            {
                instances.Add(_storage.Get());
            }

            foreach (GameObject instance in instances)
            {
                _storage.Release(instance);
            }
        }

        /// <summary>
        /// Destroys every inactive instance retained by the underlying Unity pool.
        /// 내부 Unity 풀이 보관 중인 모든 비활성 인스턴스를 파괴합니다.
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            _storage.Clear();
        }

        /// <summary>
        /// Destroys active and inactive instances owned by this pool.
        /// 이 풀이 소유한 활성 및 비활성 인스턴스를 모두 파괴합니다.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _storage.Clear();
            foreach (GameObject instance in _activeInstances)
            {
                DestroyUnityObject(instance);
            }

            _activeInstances.Clear();
            DestroyUnityObject(_root);
        }

        private GameObject CreateInstance()
        {
            GameObject instance = UnityObject.Instantiate(
                _configuration.Prefab,
                _root.transform);
            _createdCount++;
            instance.SetActive(false);

            PooledGameObjectState state =
                instance.GetComponent<PooledGameObjectState>() ??
                instance.AddComponent<PooledGameObjectState>();
            state.Initialize(this);
            return instance;
        }

        private static void ApplySpawnOptions(
            Transform instance,
            in PoolSpawnOptions options)
        {
            instance.SetParent(options.Parent, false);
            if (options.HasPose)
            {
                instance.SetPositionAndRotation(options.Position, options.Rotation);
            }
        }

        private PooledGameObjectState GetState(GameObject instance)
        {
            PooledGameObjectState state = instance.GetComponent<PooledGameObjectState>();
            if (state == null || state.Owner != this)
            {
                throw new InvalidOperationException(
                    "The instance was not created by this pool.");
            }

            return state;
        }

        private void DestroyInstance(GameObject instance)
        {
            _destroyedCount++;
            DestroyUnityObject(instance);
        }

        private void HandleDestroyedRelease(GameObject instance)
        {
            _activeInstances.Remove(instance);
            _destroyedCount++;
            _invalidReleaseCount++;

            DestroyedInstancePolicy policy = _configuration.DestroyedInstancePolicy;
            if (policy == DestroyedInstancePolicy.Throw)
            {
                throw new InvalidOperationException(
                    "An active pooled instance was destroyed before release.");
            }

            if (policy == DestroyedInstancePolicy.WarnAndReplace)
            {
                Debug.LogWarning(
                    $"[{nameof(UnityGameObjectPool)}] A destroyed active instance could not " +
                    $"be returned to {_configuration.Name}; the next Get will create a replacement.");
            }
        }

        private void RemoveDestroyedActiveInstances()
        {
            int removedCount = _activeInstances.RemoveWhere(instance => instance == null);
            _destroyedCount += removedCount;
        }

        private static void DestroyUnityObject(GameObject instance)
        {
            if (instance == null) return;

            if (Application.isPlaying)
            {
                UnityObject.Destroy(instance);
            }
            else
            {
                UnityObject.DestroyImmediate(instance);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UnityGameObjectPool));
            }
        }
    }
}
