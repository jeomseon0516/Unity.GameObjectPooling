namespace Jeomseon.Unity.GameObjectPooling.Configurations
{
    /// <summary>
    /// Marks immutable runtime data used to construct a GameObject pool. Implementations
    /// should contain construction settings only and must not depend on ScriptableObject.
    /// GameObject 풀 생성에 사용하는 불변 런타임 데이터를 나타냅니다. 구현체는 생성 설정만
    /// 포함해야 하며 ScriptableObject에 의존하지 않아야 합니다.
    /// </summary>
    public interface IGameObjectPoolConfiguration
    {
    }
}
