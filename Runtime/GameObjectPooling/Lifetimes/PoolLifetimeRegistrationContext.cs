using System;
using Jeomseon.GameObjectPooling.Handles;

namespace Jeomseon.GameObjectPooling.Lifetimes
{
    /// <summary>
    /// Provides the owner Scene and a restricted release operation to lifetime handlers.
    /// Lifetime handlers do not receive direct access to a scope's internal registry.
    /// 수명 Handler에 소유 Scene과 제한된 해제 작업을 제공합니다. 수명 Handler는 Scope의
    /// 내부 Registry에 직접 접근하지 않습니다.
    /// </summary>
    public readonly struct PoolLifetimeRegistrationContext
    {
        private readonly Action<GameObjectPoolHandle> _release;

        /// <summary>
        /// Gets the handle owner's Unity Scene handle.
        /// Handle 소유자의 Unity Scene Handle을 가져옵니다.
        /// </summary>
        public ulong OwnerSceneHandle { get; }

        internal PoolLifetimeRegistrationContext(
            ulong ownerSceneHandle,
            Action<GameObjectPoolHandle> release)
        {
            OwnerSceneHandle = ownerSceneHandle;
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        /// <summary>
        /// Requests that the owning scope release a managed handle.
        /// 소유 Scope에 관리 중인 Handle의 해제를 요청합니다.
        /// </summary>
        public void Release(GameObjectPoolHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            _release(handle);
        }
    }
}
