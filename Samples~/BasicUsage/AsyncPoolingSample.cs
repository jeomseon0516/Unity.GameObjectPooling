using System.Threading;
using Jeomseon.GameObjectPooling.Definitions;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Registrations;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Shows the high-level asynchronous workflow. Pool creation may load asynchronously,
    /// while spawning and returning remain immediate operations after registration.
    /// 고수준 비동기 사용 흐름을 보여줍니다. Pool 생성은 비동기로 로드할 수 있지만 등록이
    /// 끝난 뒤 생성과 반환은 즉시 실행되는 동기 작업으로 유지됩니다.
    /// </summary>
    public sealed class AsyncPoolingSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_scope")] private GameObjectPoolScope scope;
        [SerializeField, FormerlySerializedAs("_definition")] private GameObjectPoolDefinition definition;

        private GameObjectPoolHandle _handle;

        private async Awaitable Start()
        {
            CancellationToken cancellationToken = destroyCancellationToken;
            _handle = await scope.RegisterAsync(definition, cancellationToken);

            GameObject instance = _handle.Spawn();
            _handle.Despawn(instance);
        }

        /// <summary>
        /// Starts the same registration through the optional callback convenience API.
        /// 선택적 Callback 편의 API를 통해 같은 등록을 시작합니다.
        /// </summary>
        [ContextMenu("Register With Callback")]
        private void RegisterWithCallback()
        {
            scope.RegisterAsync(
                definition,
                OnRegistered,
                destroyCancellationToken);
        }

        private void OnRegistered(PoolRegistrationResult result)
        {
            if (result.IsCanceled) return;
            if (result.IsFailed)
            {
                Debug.LogException(result.Exception, this);
                return;
            }

            _handle = result.Handle;
        }
    }
}
