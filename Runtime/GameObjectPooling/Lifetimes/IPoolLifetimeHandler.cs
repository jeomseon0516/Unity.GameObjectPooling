using System;
using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;

namespace Jeomseon.GameObjectPooling.Lifetimes
{
    /// <summary>
    /// Executes one family of pool lifetime policies. A scope owns registered handlers and
    /// disposes them during shutdown. Custom handlers may observe project events and release
    /// their registered handles through the supplied context.
    /// 풀 수명 정책 계열 하나를 실행합니다. Scope는 등록된 Handler를 소유하고 종료 시
    /// Dispose합니다. 사용자 Handler는 프로젝트 이벤트를 관찰하고 제공된 Context를 통해
    /// 등록 Handle을 해제할 수 있습니다.
    /// </summary>
    public interface IPoolLifetimeHandler : IDisposable
    {
        /// <summary>
        /// Returns whether this handler supports the supplied configuration.
        /// 이 Handler가 지정된 Configuration을 지원하는지 반환합니다.
        /// </summary>
        bool CanHandle(IPoolLifetimeConfiguration configuration);

        /// <summary>
        /// Validates a configuration before the runtime pool is created.
        /// 런타임 풀이 생성되기 전에 Configuration을 검증합니다.
        /// </summary>
        void Validate(IPoolLifetimeConfiguration configuration);

        /// <summary>
        /// Starts managing a handle with the supplied lifetime context.
        /// 지정된 수명 Context로 Handle 관리를 시작합니다.
        /// </summary>
        void Register(
            GameObjectPoolHandle handle,
            IPoolLifetimeConfiguration configuration,
            in PoolLifetimeRegistrationContext context);

        /// <summary>
        /// Stops managing a handle without releasing it.
        /// Handle을 해제하지 않고 수명 관리를 중단합니다.
        /// </summary>
        void Unregister(GameObjectPoolHandle handle);
    }
}
