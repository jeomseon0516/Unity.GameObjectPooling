namespace Jeomseon.GameObjectPooling.Contracts
{
    /// <summary>
    /// Represents an immutable snapshot of pool statistics.
    /// 풀 통계의 불변 스냅샷을 나타냅니다.
    /// </summary>
    public readonly struct PoolStatistics
    {
        /// <summary>Gets the active instance count. 활성 인스턴스 개수를 가져옵니다.</summary>
        public int CountActive { get; }

        /// <summary>Gets the retained inactive instance count. 보관 중인 비활성 인스턴스 개수를 가져옵니다.</summary>
        public int CountInactive { get; }

        /// <summary>Gets the cumulative created instance count. 누적 생성 인스턴스 개수를 가져옵니다.</summary>
        public int CreatedCount { get; }

        /// <summary>Gets the cumulative destroyed instance count. 누적 파괴 인스턴스 개수를 가져옵니다.</summary>
        public int DestroyedCount { get; }

        /// <summary>Gets the cumulative successful release count. 누적 정상 반환 횟수를 가져옵니다.</summary>
        public int ReleasedCount { get; }

        /// <summary>Gets the cumulative invalid release count. 누적 잘못된 반환 횟수를 가져옵니다.</summary>
        public int InvalidReleaseCount { get; }

        /// <summary>
        /// Gets the number of releases discarded because the inactive capacity was full.
        /// 비활성 용량이 가득 차 폐기된 반환 횟수를 가져옵니다.
        /// </summary>
        public int CapacityDiscardedCount { get; }

        internal PoolStatistics(
            int countActive,
            int countInactive,
            int createdCount,
            int destroyedCount,
            int releasedCount,
            int invalidReleaseCount,
            int capacityDiscardedCount)
        {
            CountActive = countActive;
            CountInactive = countInactive;
            CreatedCount = createdCount;
            DestroyedCount = destroyedCount;
            ReleasedCount = releasedCount;
            InvalidReleaseCount = invalidReleaseCount;
            CapacityDiscardedCount = capacityDiscardedCount;
        }
    }
}
