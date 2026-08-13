namespace Jeomseon.Unity.GameObjectPooling.Lifecycle
{
    /// <summary>
    /// Receives a notification after a pooled GameObject is activated.
    /// 풀링된 GameObject가 활성화된 후 알림을 받습니다.
    /// </summary>
    public interface IPoolGetHandler
    {
        /// <summary>
        /// Called after the instance has received its spawn transform and activation.
        /// 인스턴스에 생성 Transform이 적용되고 활성화된 후 호출됩니다.
        /// </summary>
        void OnGetFromPool();
    }
}
