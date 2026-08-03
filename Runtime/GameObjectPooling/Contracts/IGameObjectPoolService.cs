using System;
using System.Threading;
using Jeomseon.GameObjectPooling.Configurations;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Contracts
{
    /// <summary>
    /// Creates and owns runtime pools while hiding their concrete implementation.
    /// 구체 구현을 숨기면서 런타임 풀을 생성하고 소유합니다.
    /// </summary>
    public interface IGameObjectPoolService : IDisposable
    {
        /// <summary>
        /// Creates and owns a pool directly from runtime configuration without a Definition.
        /// Definition 없이 런타임 Configuration으로 풀을 직접 생성하고 소유합니다.
        /// </summary>
        IGameObjectPool CreatePool(IGameObjectPoolConfiguration configuration);

        /// <summary>
        /// Creates and owns a pool asynchronously. An asynchronous factory is preferred; a
        /// synchronous factory is used as a non-blocking fallback when none supports the input.
        /// 풀을 비동기로 생성하고 소유합니다. 비동기 Factory를 우선하며 지원하는 비동기
        /// Factory가 없으면 동기 Factory를 블로킹 없이 fallback으로 사용합니다.
        /// </summary>
        Awaitable<IGameObjectPool> CreatePoolAsync(
            IGameObjectPoolConfiguration configuration,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes and disposes a pool owned by this service.
        /// 이 서비스가 소유한 풀을 제거하고 해제합니다.
        /// </summary>
        bool ReleasePool(IGameObjectPool pool);

        /// <summary>
        /// Registers a factory used for custom configuration and pool implementations.
        /// 사용자 Configuration과 풀 구현에 사용할 Factory를 등록합니다.
        /// </summary>
        void RegisterFactory(IGameObjectPoolFactory factory);

        /// <summary>
        /// Registers an asynchronous factory. Later factories take precedence.
        /// 비동기 Factory를 등록합니다. 나중에 등록한 Factory가 우선합니다.
        /// </summary>
        void RegisterAsyncFactory(IAsyncGameObjectPoolFactory factory);
    }
}
