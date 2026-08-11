using Jeomseon.GameObjectPooling.Contracts;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Demonstrates concise Definition-based access through a scene scope's default handle.
    /// 씬 Scope의 기본 Handle을 통한 간결한 Definition 기반 접근을 보여줍니다.
    /// </summary>
    public sealed class GameObjectPoolingSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_scope")] private GameObjectPoolScope scope;
        private GameObject _instance;

        [ContextMenu("Get Pooled Object / 풀 객체 가져오기")]
        private void GetPooledObject()
        {
            if (_instance != null || scope == null) return;

            _instance = scope.Spawn(PoolSpawnOptions.At(
                transform.position,
                transform.rotation,
                transform));
        }

        [ContextMenu("Release Pooled Object / 풀 객체 반환하기")]
        private void ReleasePooledObject()
        {
            if (_instance == null || scope == null) return;

            scope.Despawn(_instance);
            _instance = null;
        }
    }
}
