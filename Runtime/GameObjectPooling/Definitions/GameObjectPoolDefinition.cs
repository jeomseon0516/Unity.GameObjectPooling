using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Registrations;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Definitions
{
    /// <summary>
    /// Identifies a GameObject pool and provides the base type used by pool factories.
    /// Derive a new definition together with a matching factory when a custom pooling
    /// implementation requires different serialized settings.
    /// GameObject 풀을 식별하고 풀 Factory가 사용하는 기반 타입을 제공합니다. 사용자 풀링
    /// 구현에 다른 직렬화 설정이 필요하면 대응하는 Factory와 함께 새 Definition을 상속합니다.
    /// </summary>
    public abstract class GameObjectPoolDefinition : ScriptableObject
    {
        /// <summary>
        /// Creates an immutable configuration consumed by runtime factories and pools.
        /// Runtime objects never retain this ScriptableObject definition.
        /// 런타임 Factory와 풀이 사용하는 불변 Configuration을 생성합니다. 런타임 객체는
        /// 이 ScriptableObject Definition을 보관하지 않습니다.
        /// </summary>
        public abstract IGameObjectPoolConfiguration CreateConfiguration();

        /// <summary>
        /// Creates immutable orchestration settings consumed by the pool host. Runtime pools
        /// never retain this lifetime configuration.
        /// Pool Host가 사용하는 불변 오케스트레이션 설정을 생성합니다. 런타임 풀은 이 수명
        /// Configuration을 보관하지 않습니다.
        /// </summary>
        public abstract IPoolLifetimeConfiguration CreateLifetimeConfiguration();

        /// <summary>
        /// Creates a shared registration for this serialized definition. Re-registering the
        /// same Definition in one scope resolves the same runtime handle.
        /// 이 직렬화 Definition의 공유 Registration을 생성합니다. 한 Scope에서 같은
        /// Definition을 다시 등록하면 같은 런타임 Handle을 조회합니다.
        /// </summary>
        public GameObjectPoolRegistration CreateRegistration()
        {
            return new GameObjectPoolRegistration(
                CreateConfiguration(),
                CreateLifetimeConfiguration(),
                name,
                this);
        }
    }
}
