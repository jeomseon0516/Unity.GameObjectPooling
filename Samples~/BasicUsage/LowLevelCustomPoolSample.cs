using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    // LOW-LEVEL CUSTOMIZATION / 저수준 사용자 정의
    // Register a custom factory before the Scope registers its default pool.
    // Scope가 기본 풀을 등록하기 전에 사용자 Factory를 등록합니다.

    /// <summary>
    /// Registers the low-level sample factory and configures a host before consumers use it.
    /// 소비자가 사용하기 전에 저수준 샘플 Factory를 등록하고 Host를 구성합니다.
    /// </summary>
    [DefaultExecutionOrder(-1100)]
    public sealed class LowLevelCustomPoolSample : MonoBehaviour
    {
        [SerializeField] private GameObjectPoolScope _scope;
        [SerializeField] private GameObject _prefab;

        private void Awake()
        {
            if (_scope == null || _prefab == null) return;

            _scope.RegisterFactory(new CountingGameObjectPoolFactory());
            var unityConfiguration = new UnityGameObjectPoolConfiguration(
                _prefab,
                "Low-Level Counting Pool");
            GameObjectPoolHandle handle = _scope.Register(
                new CountingPoolConfiguration(unityConfiguration),
                PoolLifetimeConfiguration.Scene);
            _scope.SetDefault(handle);
        }
    }
}
