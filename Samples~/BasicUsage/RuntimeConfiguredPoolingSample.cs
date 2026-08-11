using Jeomseon.GameObjectPooling.Contracts;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Demonstrates runtime pool creation with separate construction and lifetime
    /// configurations, without a ScriptableObject Definition.
    /// ScriptableObject Definition 없이 생성 Configuration과 수명 Configuration을 분리하여
    /// 런타임 풀을 만드는 방법을 보여줍니다.
    /// </summary>
    public sealed class RuntimeConfiguredPoolingSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_scope")] private GameObjectPoolScope scope;
        private GameObjectPoolHandle _handle;
        private GameObject _instance;

        [ContextMenu("Get Runtime Pooled Object / 런타임 풀 객체 가져오기")]
        private void GetPooledObject()
        {
            if (_instance != null) return;

            if (scope == null) return;

            _handle ??= scope.DefaultHandle;
            _instance = _handle.Spawn(PoolSpawnOptions.At(
                transform.position,
                transform.rotation,
                transform));
        }

        [ContextMenu("Release Runtime Pooled Object / 런타임 풀 객체 반환")]
        private void ReleasePooledObject()
        {
            if (_instance == null || _handle == null) return;

            _handle.Despawn(_instance);
            _instance = null;
        }
    }
}
