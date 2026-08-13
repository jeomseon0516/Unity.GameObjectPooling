using Jeomseon.Unity.GameObjectPooling.Configurations;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Identifies the project round that owns a runtime pool.
    /// 런타임 풀을 소유하는 프로젝트 Round를 식별합니다.
    /// </summary>
    public sealed class RoundLifetimeConfiguration : IPoolLifetimeConfiguration
    {
        /// <summary>
        /// Gets the owning round identifier.
        /// 소유 Round 식별자를 가져옵니다.
        /// </summary>
        public int RoundId { get; }

        /// <summary>
        /// Creates a round lifetime configuration.
        /// Round 수명 Configuration을 생성합니다.
        /// </summary>
        public RoundLifetimeConfiguration(int roundId)
        {
            RoundId = roundId;
        }
    }
}
