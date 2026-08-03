using System;
using Jeomseon.GameObjectPooling.Contracts;
using Jeomseon.GameObjectPooling.Handles;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Providers
{
    /// <summary>
    /// Provides type-safe Spawn and Despawn operations over a GameObject pool without owning
    /// another storage. Derived providers can add project-specific initialization through the
    /// protected lifecycle hooks.
    /// 별도 저장소를 소유하지 않고 GameObject 풀에 타입 안전한 Spawn 및 Despawn 작업을
    /// 제공합니다. 파생 Provider는 보호된 수명 훅을 통해 프로젝트별 초기화를 추가할 수 있습니다.
    /// </summary>
    /// <typeparam name="T">
    /// The required component type on the pooled prefab.
    /// 풀링 Prefab에 필요한 Component 타입입니다.
    /// </typeparam>
    public class ComponentPoolProvider<T> where T : Component
    {
        private readonly GameObjectPoolHandle _handle;

        /// <summary>
        /// Creates a typed Provider over a scope-owned runtime handle.
        /// Scope가 소유한 런타임 Handle을 사용하는 타입 지정 Provider를 생성합니다.
        /// </summary>
        public ComponentPoolProvider(GameObjectPoolHandle handle)
        {
            _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        /// <summary>
        /// Spawns a component and invokes the provider extension hook.
        /// Component를 생성하고 Provider 확장 훅을 호출합니다.
        /// </summary>
        public T Spawn()
        {
            T instance = _handle.Spawn<T>();
            OnSpawned(instance);
            return instance;
        }

        /// <summary>
        /// Spawns a component with the supplied transform options.
        /// 지정된 Transform 옵션으로 Component를 생성합니다.
        /// </summary>
        public T Spawn(in PoolSpawnOptions options)
        {
            T instance = _handle.Spawn<T>(options);
            OnSpawned(instance);
            return instance;
        }

        /// <summary>
        /// Despawns a component through the protected provider hooks.
        /// 보호된 Provider 훅을 거쳐 Component를 반환합니다.
        /// </summary>
        public void Despawn(T instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            OnDespawning(instance);
            _handle.Despawn(instance);

            OnDespawned(instance);
        }

        /// <summary>
        /// Called after an instance has been spawned.
        /// 인스턴스가 생성된 후 호출됩니다.
        /// </summary>
        protected virtual void OnSpawned(T instance) { }

        /// <summary>
        /// Called immediately before an instance is despawned.
        /// 인스턴스가 반환되기 직전에 호출됩니다.
        /// </summary>
        protected virtual void OnDespawning(T instance) { }

        /// <summary>
        /// Called after an instance has been despawned successfully.
        /// 인스턴스가 성공적으로 반환된 후 호출됩니다.
        /// </summary>
        protected virtual void OnDespawned(T instance) { }
    }
}
