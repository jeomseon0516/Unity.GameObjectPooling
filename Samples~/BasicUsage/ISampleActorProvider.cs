using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    // HIGH-LEVEL CUSTOMIZATION / 고수준 사용자 정의
    // Gameplay depends only on this domain contract and never sees a pool or Definition.
    // 게임 로직은 이 도메인 계약에만 의존하며 Pool이나 Definition을 직접 보지 않습니다.

    /// <summary>
    /// Defines the project-facing creation and destruction boundary. Gameplay code depends
    /// on this contract instead of a pooling implementation.
    /// 프로젝트 호출부가 사용하는 생성·파괴 경계를 정의합니다. 게임 로직은 풀 구현체 대신
    /// 이 계약에 의존합니다.
    /// </summary>
    public interface ISampleActorProvider
    {
        /// <summary>
        /// Spawns and initializes an actor for gameplay.
        /// 게임플레이에 사용할 Actor를 생성하고 초기화합니다.
        /// </summary>
        PooledSampleActor Spawn(
            string actorName,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);

        /// <summary>
        /// Despawns an actor according to the provider's backing policy.
        /// Provider의 내부 정책에 따라 Actor를 제거합니다.
        /// </summary>
        void Despawn(PooledSampleActor actor);
    }
}
