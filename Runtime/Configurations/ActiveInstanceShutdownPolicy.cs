namespace Jeomseon.Unity.GameObjectPooling.Configurations
{
    /// <summary>
    /// Defines what happens to active instances when their owning pool shuts down.
    /// 소유 Pool이 종료될 때 활성 인스턴스를 처리하는 방식을 정의합니다.
    /// </summary>
    public enum ActiveInstanceShutdownPolicy
    {
        /// <summary>
        /// Destroys active instances together with the pool.
        /// Pool과 함께 활성 인스턴스를 파괴합니다.
        /// </summary>
        Destroy = 0,

        /// <summary>
        /// Leaves active instances alive after detaching them from the pool.
        /// 활성 인스턴스를 Pool에서 분리해 살아 있게 둡니다.
        /// </summary>
        Preserve = 1
    }
}
