using System;
using Jeomseon.Unity.GameObjectPooling.Configurations;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Wraps Unity pool settings to select the sample custom factory.
    /// 샘플 사용자 Factory를 선택할 수 있도록 Unity 풀 설정을 감쌉니다.
    /// </summary>
    public sealed class CountingPoolConfiguration : IGameObjectPoolConfiguration
    {
        /// <summary>
        /// Gets the settings delegated to Unity's default pool implementation.
        /// Unity 기본 풀 구현에 위임할 설정을 가져옵니다.
        /// </summary>
        public UnityGameObjectPoolConfiguration InnerConfiguration { get; }

        /// <summary>
        /// Creates a counting-pool configuration.
        /// Counting Pool Configuration을 생성합니다.
        /// </summary>
        public CountingPoolConfiguration(UnityGameObjectPoolConfiguration innerConfiguration)
        {
            InnerConfiguration = innerConfiguration ??
                throw new ArgumentNullException(nameof(innerConfiguration));
        }
    }
}
