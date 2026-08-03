using System.Threading;
using Jeomseon.GameObjectPooling.Configurations;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Contracts
{
    /// <summary>
    /// Creates pools whose resources require asynchronous loading. Each returned Awaitable is
    /// consumed once by the service and must transfer resource ownership to the returned pool.
    /// 비동기 로딩이 필요한 리소스의 풀을 생성합니다. 반환된 Awaitable은 Service가 한 번만
    /// 소비하며, 로드한 리소스의 소유권은 반환되는 Pool로 이전해야 합니다.
    /// </summary>
    public interface IAsyncGameObjectPoolFactory
    {
        /// <summary>
        /// Returns whether this factory supports the supplied runtime configuration.
        /// 이 Factory가 지정된 런타임 Configuration을 지원하는지 반환합니다.
        /// </summary>
        bool CanCreate(IGameObjectPoolConfiguration configuration);

        /// <summary>
        /// Asynchronously creates a pool beneath the optional runtime root.
        /// 선택적 런타임 루트 아래에 풀을 비동기로 생성합니다.
        /// </summary>
        Awaitable<IGameObjectPool> CreateAsync(
            IGameObjectPoolConfiguration configuration,
            Transform runtimeRoot = null,
            CancellationToken cancellationToken = default);
    }
}
