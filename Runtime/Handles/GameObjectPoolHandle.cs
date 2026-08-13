using System;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Handles
{
    /// <summary>
    /// Represents one pool registered in a scope. Consumers retain this handle instead of a
    /// Definition or configuration, so serialized and runtime registrations use the same API.
    /// Scope에 등록된 하나의 풀을 나타냅니다. 소비자는 Definition이나 Configuration 대신
    /// 이 Handle을 보관하므로 직렬화 및 런타임 등록이 같은 API를 사용합니다.
    /// </summary>
    public sealed class GameObjectPoolHandle
    {
        private IGameObjectPool _pool;

        internal IGameObjectPool Pool => _pool ??
            throw new ObjectDisposedException(nameof(GameObjectPoolHandle));

        /// <summary>
        /// Gets the diagnostic registration name.
        /// 등록된 진단용 이름을 가져옵니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether the owning scope still manages this pool.
        /// 소유 Scope가 이 풀을 계속 관리하는지 가져옵니다.
        /// </summary>
        public bool IsValid => _pool != null;

        /// <summary>
        /// Tries to read diagnostics from the underlying pool.
        /// 내부 풀에서 진단 정보를 가져옵니다.
        /// </summary>
        public bool TryGetStatistics(out PoolStatistics statistics)
        {
            if (_pool is IPoolDiagnostics diagnostics)
            {
                statistics = diagnostics.Statistics;
                return true;
            }

            statistics = default;
            return false;
        }

        internal GameObjectPoolHandle(string name, IGameObjectPool pool)
        {
            Name = name;
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        /// <summary>
        /// Spawns a GameObject.
        /// GameObject를 생성합니다.
        /// </summary>
        public GameObject Spawn() => Pool.Get();

        /// <summary>
        /// Spawns a GameObject with transform options.
        /// Transform 옵션으로 GameObject를 생성합니다.
        /// </summary>
        public GameObject Spawn(in PoolSpawnOptions options) => Pool.Get(options);

        /// <summary>
        /// Spawns the requested component type.
        /// 요청한 Component 타입을 생성합니다.
        /// </summary>
        public T Spawn<T>() where T : Component => Pool.Get<T>();

        /// <summary>
        /// Spawns the requested component type with transform options.
        /// Transform 옵션으로 요청한 Component 타입을 생성합니다.
        /// </summary>
        public T Spawn<T>(in PoolSpawnOptions options) where T : Component =>
            Pool.Get<T>(options);

        /// <summary>
        /// Returns a GameObject to the pool.
        /// GameObject를 풀에 반환합니다.
        /// </summary>
        public void Despawn(GameObject instance) => Pool.Release(instance);

        /// <summary>
        /// Returns a component's GameObject to the pool.
        /// Component의 GameObject를 풀에 반환합니다.
        /// </summary>
        public void Despawn<T>(T component) where T : Component => Pool.Release(component);

        internal void Invalidate()
        {
            _pool = null;
        }
    }
}
