using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    // MID-LEVEL CUSTOMIZATION / 중간 수준 사용자 정의
    // Configure construction and lifetime while keeping storage and ownership in the module.
    // 저장소와 소유권은 모듈에 맡기고 생성 및 수명 설정만 구성합니다.

    /// <summary>
    /// Registers runtime values at the composition boundary and assigns the returned handle
    /// as the scope default. Consumers do not depend on the creation input.
    /// Composition 경계에서 런타임 값을 등록하고 반환된 Handle을 Scope 기본값으로 지정합니다.
    /// 소비자는 생성 입력에 의존하지 않습니다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class RuntimeGameObjectPoolScopeConfigurator : MonoBehaviour
    {
        [SerializeField] private GameObjectPoolScope _scope;
        [SerializeField] private GameObject _prefab;
        [SerializeField, Min(0)] private int _prewarmCount = 2;

        private void Awake()
        {
            if (_scope == null || _prefab == null) return;

            var configuration = new UnityGameObjectPoolConfiguration(
                _prefab,
                $"{gameObject.name} Pool",
                _prewarmCount);
            GameObjectPoolHandle handle = _scope.Register(
                configuration,
                PoolLifetimeConfiguration.Scene);
            _scope.SetDefault(handle);
        }
    }
}
