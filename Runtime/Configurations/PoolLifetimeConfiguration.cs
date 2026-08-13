using System;

namespace Jeomseon.Unity.GameObjectPooling.Configurations
{
    /// <summary>
    /// Contains immutable settings for a built-in Scope, Scene, or Application lifetime.
    /// Scope, Scene 또는 Application 기본 수명의 불변 설정을 보관합니다.
    /// </summary>
    public sealed class PoolLifetimeConfiguration : IPoolLifetimeConfiguration
    {
        /// <summary>
        /// Gets the selected built-in lifetime boundary.
        /// 선택된 기본 수명 경계를 가져옵니다.
        /// </summary>
        public PoolLifetime Lifetime { get; }

        /// <summary>
        /// Gets the shared Scope lifetime configuration.
        /// 공유 Scope 수명 Configuration을 가져옵니다.
        /// </summary>
        public static PoolLifetimeConfiguration Scope { get; } =
            new(PoolLifetime.Scope);

        /// <summary>
        /// Gets the shared Scene lifetime configuration.
        /// 공유 Scene 수명 Configuration을 가져옵니다.
        /// </summary>
        public static PoolLifetimeConfiguration Scene { get; } =
            new(PoolLifetime.Scene);

        /// <summary>
        /// Gets the shared Application lifetime configuration.
        /// 공유 Application 수명 Configuration을 가져옵니다.
        /// </summary>
        public static PoolLifetimeConfiguration Application { get; } =
            new(PoolLifetime.Application);

        /// <summary>
        /// Creates a built-in lifetime configuration.
        /// 기본 수명 Configuration을 생성합니다.
        /// </summary>
        public PoolLifetimeConfiguration(PoolLifetime lifetime)
        {
            if (!Enum.IsDefined(typeof(PoolLifetime), lifetime))
            {
                throw new ArgumentOutOfRangeException(nameof(lifetime));
            }

            Lifetime = lifetime;
        }

        /// <summary>
        /// Returns a shared immutable configuration for the selected lifetime.
        /// 선택된 수명에 대응하는 공유 불변 Configuration을 반환합니다.
        /// </summary>
        public static PoolLifetimeConfiguration From(PoolLifetime lifetime)
        {
            return lifetime switch
            {
                PoolLifetime.Scope => Scope,
                PoolLifetime.Scene => Scene,
                PoolLifetime.Application => Application,
                _ => throw new ArgumentOutOfRangeException(nameof(lifetime))
            };
        }
    }
}
