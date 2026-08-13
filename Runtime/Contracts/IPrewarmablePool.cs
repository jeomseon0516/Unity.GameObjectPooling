namespace Jeomseon.Unity.GameObjectPooling.Contracts
{
    /// <summary>
    /// Defines the optional capability to prepare inactive instances in advance.
    /// 비활성 인스턴스를 미리 준비하는 선택적 기능을 정의합니다.
    /// </summary>
    public interface IPrewarmablePool
    {
        /// <summary>Creates and retains up to the requested count. 요청 개수까지 미리 생성하여 보관합니다.</summary>
        void Prewarm(int count);
    }
}
