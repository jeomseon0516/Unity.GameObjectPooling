namespace Jeomseon.Unity.GameObjectPooling.Configurations
{
    /// <summary>
    /// Selects a built-in Unity lifetime boundary for a runtime pool.
    /// 런타임 풀에 적용할 Unity 기본 수명 경계를 선택합니다.
    /// </summary>
    public enum PoolLifetime
    {
        /// <summary>Lives until its scope is shut down. Scope가 종료될 때까지 유지됩니다.</summary>
        Scope,

        /// <summary>Ends when its last owning scene unloads. 마지막 소유 Scene이 종료될 때 해제됩니다.</summary>
        Scene,

        /// <summary>Lives with a persistent scope until application shutdown. 영속 Scope와 함께 애플리케이션 종료까지 유지됩니다.</summary>
        Application
    }
}
