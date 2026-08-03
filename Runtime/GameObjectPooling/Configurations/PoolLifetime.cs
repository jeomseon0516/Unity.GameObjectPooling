namespace Jeomseon.GameObjectPooling.Configurations
{
    /// <summary>
    /// Selects a built-in Unity lifetime boundary for a runtime pool.
    /// 런타임 풀에 적용할 Unity 기본 수명 경계를 선택합니다.
    /// </summary>
    public enum PoolLifetime
    {
        /* TODO(P2, owner-pool-lifetime): Add an Owner lifetime as a separate
         * IPoolLifetimeConfiguration and IPoolLifetimeHandler pair instead of storing a
         * UnityEngine.Object in this shared enum configuration.
         * Owner 수명을 이 공유 enum Configuration에 UnityEngine.Object를 저장하는 방식이
         * 아니라 별도의 IPoolLifetimeConfiguration/IPoolLifetimeHandler 쌍으로 추가합니다.
         * - Accept a GameObject or Component as the owner. / GameObject 또는 Component를 소유자로 받습니다.
         * - Release every registered handle when the owner is actually destroyed, not merely disabled.
         *   단순 비활성화가 아니라 소유자가 실제로 파괴될 때 등록된 모든 Handle을 해제합니다.
         * - Reject an owner that is already destroyed and support multiple handles per owner.
         *   이미 파괴된 소유자는 거부하고 소유자 하나에 여러 Handle 등록을 지원합니다.
         */

        /// <summary>Lives until its scope is shut down. Scope가 종료될 때까지 유지됩니다.</summary>
        Scope,

        /// <summary>Ends when its last owning scene unloads. 마지막 소유 Scene이 종료될 때 해제됩니다.</summary>
        Scene,

        /// <summary>Lives with a persistent scope until application shutdown. 영속 Scope와 함께 애플리케이션 종료까지 유지됩니다.</summary>
        Application
    }
}
