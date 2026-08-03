namespace Jeomseon.GameObjectPooling.Contracts
{
    /// <summary>
    /// Exposes read-only runtime statistics without making diagnostics mandatory for every pool.
    /// 모든 풀에 진단을 강제하지 않고 읽기 전용 런타임 통계를 노출합니다.
    /// </summary>
    public interface IPoolDiagnostics
    {
        /// <summary>Gets the current and cumulative statistics. 현재 및 누적 통계를 가져옵니다.</summary>
        PoolStatistics Statistics { get; }
    }
}
