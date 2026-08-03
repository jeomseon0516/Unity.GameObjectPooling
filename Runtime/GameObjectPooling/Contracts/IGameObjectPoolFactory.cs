using Jeomseon.GameObjectPooling.Configurations;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Contracts
{
    /// <summary>
    /// Creates pools for supported configurations. Custom pooling systems can participate in
    /// pool management by supplying another implementation of this interface.
    /// 지원하는 Configuration의 풀을 생성합니다. 사용자 풀링 시스템은 이 인터페이스의 다른
    /// 구현을 제공하여 풀 관리에 참여할 수 있습니다.
    /// </summary>
    public interface IGameObjectPoolFactory
    {
        /// <summary>
        /// Returns whether this factory supports the supplied runtime configuration.
        /// 이 Factory가 지정된 런타임 Configuration을 지원하는지 반환합니다.
        /// </summary>
        bool CanCreate(IGameObjectPoolConfiguration configuration);

        /// <summary>
        /// Creates a pool beneath the optional runtime root.
        /// 선택적 런타임 루트 아래에 풀을 생성합니다.
        /// </summary>
        IGameObjectPool Create(
            IGameObjectPoolConfiguration configuration,
            Transform runtimeRoot = null);
    }
}
