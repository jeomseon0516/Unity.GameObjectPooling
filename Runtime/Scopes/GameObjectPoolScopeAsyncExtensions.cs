using System;
using System.Threading;
using Jeomseon.Unity.GameObjectPooling.Definitions;
using Jeomseon.Unity.GameObjectPooling.Registrations;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Scopes
{
    /// <summary>
    /// Provides callback-based convenience APIs over the canonical Awaitable registration API.
    /// 표준 Awaitable 등록 API 위에 Callback 기반 편의 API를 제공합니다.
    /// </summary>
    public static class GameObjectPoolScopeAsyncExtensions
    {
        /// <summary>
        /// Registers a Definition asynchronously and delivers exactly one terminal result on
        /// the Unity main thread. Exceptions thrown by the callback propagate to the caller.
        /// Definition을 비동기로 등록하고 Unity 메인 스레드에서 최종 결과를 정확히 한 번
        /// 전달합니다. Callback에서 발생한 예외는 호출자에게 전파됩니다.
        /// </summary>
        public static Awaitable RegisterAsync(
            this GameObjectPoolScope scope,
            GameObjectPoolDefinition definition,
            Action<PoolRegistrationResult> callback,
            CancellationToken cancellationToken = default)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return RegisterAsync(
                scope,
                definition.CreateRegistration(),
                callback,
                cancellationToken);
        }

        /// <summary>
        /// Registers creation input asynchronously and delivers exactly one terminal result on
        /// the Unity main thread. The Scope remains the owner of a successful Handle and Pool.
        /// 생성 입력을 비동기로 등록하고 Unity 메인 스레드에서 최종 결과를 정확히 한 번
        /// 전달합니다. 성공한 Handle과 Pool의 소유권은 Scope에 유지됩니다.
        /// </summary>
        public static async Awaitable RegisterAsync(
            this GameObjectPoolScope scope,
            GameObjectPoolRegistration registration,
            Action<PoolRegistrationResult> callback,
            CancellationToken cancellationToken = default)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            PoolRegistrationResult result;
            try
            {
                result = PoolRegistrationResult.Success(
                    await scope.RegisterAsync(registration, cancellationToken));
            }
            catch (OperationCanceledException exception)
            {
                result = PoolRegistrationResult.Canceled(exception);
            }
            catch (Exception exception)
            {
                result = PoolRegistrationResult.Failure(exception);
            }

            await Awaitable.MainThreadAsync();
            callback(result);
        }
    }
}
