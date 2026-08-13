using Jeomseon.Unity.GameObjectPooling.Lifecycle;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Represents a project-domain object used by the Provider sample.
    /// Provider 샘플에서 사용하는 프로젝트 도메인 객체를 나타냅니다.
    /// </summary>
    public sealed class PooledSampleActor : MonoBehaviour, IPoolable
    {
        private string _actorName;

        /// <summary>
        /// Applies domain data after this actor is spawned.
        /// 이 Actor가 생성된 후 도메인 데이터를 적용합니다.
        /// </summary>
        public void Initialize(string actorName)
        {
            _actorName = actorName;
            gameObject.name = actorName;
        }

        /// <inheritdoc />
        public void OnGetFromPool()
        {
            Debug.Log("Actor obtained from pool. / Actor를 풀에서 가져왔습니다.");
        }

        /// <inheritdoc />
        public void OnReleaseToPool()
        {
            Debug.Log($"Actor returned to pool. / Actor를 풀에 반환했습니다: {_actorName}");
            _actorName = string.Empty;
        }
    }
}
