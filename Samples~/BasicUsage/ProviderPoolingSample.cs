using Jeomseon.Unity.GameObjectPooling.Scopes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.GameObjectPooling
{
    // HIGH-LEVEL CALL SITE / 고수준 호출부
    // Only Spawn and Despawn are used after the Provider is composed with a scope handle.
    // Scope Handle로 Provider를 조립한 뒤에는 Spawn과 Despawn만 사용합니다.

    /// <summary>
    /// Demonstrates a gameplay call site that depends only on a domain Provider while its
    /// backing pool can come from either a Definition or runtime configuration.
    /// 게임 호출부는 도메인 Provider에만 의존하고 내부 풀은 Definition 또는 런타임
    /// Configuration에서 선택할 수 있는 구조를 보여줍니다.
    /// </summary>
    public sealed class ProviderPoolingSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_scope")] private GameObjectPoolScope scope;
        private ISampleActorProvider _provider;
        private PooledSampleActor _actor;

        [ContextMenu("Spawn Through Provider / Provider로 생성")]
        private void Spawn()
        {
            if (_actor != null) return;

            EnsureProvider();
            if (_provider == null) return;

            _actor = _provider.Spawn(
                "Provider Actor",
                transform.position,
                transform.rotation,
                transform);
        }

        [ContextMenu("Despawn Through Provider / Provider로 제거")]
        private void Despawn()
        {
            if (_actor == null || _provider == null) return;

            _provider.Despawn(_actor);
            _actor = null;
        }

        private void EnsureProvider()
        {
            if (_provider != null || scope == null) return;

            _provider = new SampleActorPoolProvider(scope.DefaultHandle);
        }
    }
}
