namespace Jeomseon.Unity.GameObjectPooling.Lifecycle
{
    /// <summary>
    /// Combines the get and release lifecycle callbacks for pooled components.
    /// 풀링된 컴포넌트의 대여 및 반환 생명주기 콜백을 결합합니다.
    /// </summary>
    public interface IPoolable : IPoolGetHandler, IPoolReleaseHandler
    {
    }
}
