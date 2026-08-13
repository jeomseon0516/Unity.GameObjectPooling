namespace Jeomseon.Unity.GameObjectPooling.Configurations
{
    /// <summary>
    /// Marks immutable orchestration data that determines who releases a runtime pool.
    /// Pool implementations do not consume or retain this configuration.
    /// 런타임 풀을 누가 해제할지 결정하는 불변 오케스트레이션 데이터를 나타냅니다.
    /// 풀 구현체는 이 Configuration을 사용하거나 보관하지 않습니다.
    /// </summary>
    public interface IPoolLifetimeConfiguration
    {
    }
}
