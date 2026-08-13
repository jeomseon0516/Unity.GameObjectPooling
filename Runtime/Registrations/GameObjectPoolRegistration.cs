using System;
using Jeomseon.Unity.GameObjectPooling.Configurations;

namespace Jeomseon.Unity.GameObjectPooling.Registrations
{
    /// <summary>
    /// Describes how a scope creates and manages one pool. A registration is creation input,
    /// not the runtime pool identity returned to consumers.
    /// Scope가 하나의 풀을 생성하고 관리하는 방법을 설명합니다. Registration은 생성 입력이며,
    /// 소비자에게 반환되는 런타임 풀 식별자가 아닙니다.
    /// </summary>
    public sealed class GameObjectPoolRegistration
    {
        internal object SharedIdentity { get; }

        /// <summary>
        /// Gets the diagnostic name of this registration.
        /// 이 Registration의 진단용 이름을 가져옵니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the immutable pool construction settings.
        /// 불변 풀 생성 설정을 가져옵니다.
        /// </summary>
        public IGameObjectPoolConfiguration PoolConfiguration { get; }

        /// <summary>
        /// Gets the immutable lifetime settings.
        /// 불변 수명 설정을 가져옵니다.
        /// </summary>
        public IPoolLifetimeConfiguration LifetimeConfiguration { get; }

        /// <summary>
        /// Creates an anonymous registration. Each registration call creates an independent
        /// runtime pool, even when the same configuration instance is reused.
        /// 익명 Registration을 생성합니다. 같은 Configuration 인스턴스를 재사용해도 각 등록
        /// 호출은 독립된 런타임 풀을 생성합니다.
        /// </summary>
        public GameObjectPoolRegistration(
            IGameObjectPoolConfiguration poolConfiguration,
            IPoolLifetimeConfiguration lifetimeConfiguration,
            string name = null)
            : this(poolConfiguration, lifetimeConfiguration, name, null)
        {
        }

        internal GameObjectPoolRegistration(
            IGameObjectPoolConfiguration poolConfiguration,
            IPoolLifetimeConfiguration lifetimeConfiguration,
            string name,
            object sharedIdentity)
        {
            PoolConfiguration = poolConfiguration ??
                throw new ArgumentNullException(nameof(poolConfiguration));
            LifetimeConfiguration = lifetimeConfiguration ??
                throw new ArgumentNullException(nameof(lifetimeConfiguration));
            Name = string.IsNullOrWhiteSpace(name)
                ? poolConfiguration.GetType().Name
                : name;
            SharedIdentity = sharedIdentity;
        }
    }
}
