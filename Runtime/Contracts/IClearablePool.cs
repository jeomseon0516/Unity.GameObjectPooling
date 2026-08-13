namespace Jeomseon.Unity.GameObjectPooling.Contracts
{
    /// <summary>
    /// Defines the optional capability to discard retained inactive instances.
    /// 보관 중인 비활성 인스턴스를 폐기하는 선택적 기능을 정의합니다.
    /// </summary>
    public interface IClearablePool
    {
        /// <summary>Clears retained instances. 보관 중인 인스턴스를 정리합니다.</summary>
        void Clear();
    }
}
