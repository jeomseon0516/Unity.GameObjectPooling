using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Demonstrates a runtime pool that is released when this owner is destroyed.
    /// 이 소유자가 파괴될 때 해제되는 런타임 풀을 보여줍니다.
    /// </summary>
    public sealed class OwnerLifetimePoolingSample : MonoBehaviour
    {
        [SerializeField] private GameObjectPoolScope _scope;
        [SerializeField] private GameObject _prefab;
        private GameObjectPoolHandle _handle;

        private void Start()
        {
            if (_scope == null || _prefab == null) return;

            var poolConfiguration = new UnityGameObjectPoolConfiguration(
                _prefab,
                "Owner Lifetime Pool");
            _handle = _scope.Register(
                poolConfiguration,
                new OwnerPoolLifetimeConfiguration(this));
        }

        /// <summary>
        /// Spawns an object while this component owns the pool.
        /// 이 Component가 풀을 소유하는 동안 객체를 생성합니다.
        /// </summary>
        public GameObject Spawn() => _handle.Spawn();

        /// <summary>
        /// Returns an object to the owned pool.
        /// 객체를 소유 풀에 반환합니다.
        /// </summary>
        public void Despawn(GameObject instance) => _handle.Despawn(instance);
    }
}
