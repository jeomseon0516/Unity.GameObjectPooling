namespace Jeomseon.Unity.GameObjectPooling.Lifecycle
{
    /// <summary>
    /// Receives a notification immediately before a pooled GameObject is deactivated.
    /// 풀링된 GameObject가 비활성화되기 직전에 알림을 받습니다.
    /// </summary>
    public interface IPoolReleaseHandler
    {
        /// <summary>
        /// Called while the instance is still active and before it is stored.
        /// 인스턴스가 아직 활성 상태이고 보관되기 전에 호출됩니다.
        /// </summary>
        void OnReleaseToPool();
    }
}
