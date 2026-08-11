using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField, FormerlySerializedAs("_scope")] private GameObjectPoolScope scope;
        [SerializeField, FormerlySerializedAs("_prefab")] private GameObject prefab;

        private void Awake()
        {
            if (scope == null || prefab == null) return;

            scope.RegisterFactory(new CountingGameObjectPoolFactory());
            var unityConfiguration = new UnityGameObjectPoolConfiguration(
                prefab,
                "Low-Level Counting Pool");
            GameObjectPoolHandle handle = scope.Register(
                new CountingPoolConfiguration(unityConfiguration),
                PoolLifetimeConfiguration.Scene);
            scope.SetDefault(handle);
        }
    }
}
