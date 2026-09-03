using Jeomseon.Unity.GameObjectPooling.Configurations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.GameObjectPooling.Definitions
{
    /// <summary>
    /// Configures the default GameObject pool backed by Unity's ObjectPool.
    /// Unity ObjectPool 기반 기본 GameObject 풀을 설정합니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(UnityGameObjectPoolDefinition),
        menuName = "Tool/GameObject Pooling/Unity GameObject Pool")]
    public sealed class UnityGameObjectPoolDefinition : GameObjectPoolDefinition
    {
        [SerializeField, FormerlySerializedAs("_prefab")] private GameObject prefab;
        [SerializeField, Min(0), FormerlySerializedAs("_prewarmCount")] private int prewarmCount;
        [SerializeField, Min(1), FormerlySerializedAs("_defaultCapacity")] private int defaultCapacity = 10;
        [SerializeField, Min(1), FormerlySerializedAs("_maxInactiveCount")] private int maxInactiveCount = 100;
        [SerializeField, FormerlySerializedAs("_collectionCheck")] private bool collectionCheck = true;
        [SerializeField, FormerlySerializedAs("_resetAttributedMembers")] private bool resetAttributedMembers = true;
        [SerializeField, FormerlySerializedAs("_destroyedInstancePolicy")] private DestroyedInstancePolicy destroyedInstancePolicy =
            DestroyedInstancePolicy.WarnAndReplace;
        [SerializeField, Tooltip("Controls whether active instances are destroyed or detached when the pool shuts down.")]
        private ActiveInstanceShutdownPolicy activeInstanceShutdownPolicy =
            ActiveInstanceShutdownPolicy.Destroy;
        [SerializeField, FormerlySerializedAs("_lifetime")] private PoolLifetime lifetime = PoolLifetime.Scope;

        /// <summary>
        /// Gets the prefab instantiated when the pool is empty.
        /// 풀이 비어 있을 때 생성할 Prefab을 가져옵니다.
        /// </summary>
        public GameObject Prefab => prefab;

        /// <summary>
        /// Gets the number of inactive instances created with the pool.
        /// 풀 생성 시 미리 생성할 비활성 인스턴스 개수를 가져옵니다.
        /// </summary>
        public int PrewarmCount => prewarmCount;

        /// <summary>
        /// Gets Unity ObjectPool's initial internal collection capacity.
        /// Unity ObjectPool 내부 컬렉션의 초기 용량을 가져옵니다.
        /// </summary>
        public int DefaultCapacity => defaultCapacity;

        /// <summary>
        /// Gets the maximum inactive count. This is not a limit on active instances.
        /// 최대 비활성 보관 개수를 가져옵니다. 활성 인스턴스의 최대 개수는 아닙니다.
        /// </summary>
        public int MaxInactiveCount => maxInactiveCount;

        /// <summary>
        /// Gets whether Unity's duplicate-release validation is enabled.
        /// Unity의 중복 반환 검사가 활성화되었는지 가져옵니다.
        /// </summary>
        public bool CollectionCheck => collectionCheck;

        /// <summary>
        /// Gets whether ResetOnPoolReleaseAttribute members are restored on release.
        /// 반환 시 ResetOnPoolReleaseAttribute 멤버를 복원할지 가져옵니다.
        /// </summary>
        public bool ResetAttributedMembers => resetAttributedMembers;

        /// <summary>
        /// Gets how externally destroyed instances are handled.
        /// 외부에서 파괴된 인스턴스를 처리하는 방식을 가져옵니다.
        /// </summary>
        public DestroyedInstancePolicy DestroyedInstancePolicy => destroyedInstancePolicy;

        /// <summary>
        /// Gets how active instances are handled when the pool shuts down.
        /// Pool 종료 시 활성 인스턴스를 처리하는 방식을 가져옵니다.
        /// </summary>
        public ActiveInstanceShutdownPolicy ActiveInstanceShutdownPolicy =>
            activeInstanceShutdownPolicy;

        /// <inheritdoc />
        public override IGameObjectPoolConfiguration CreateConfiguration()
        {
            return new UnityGameObjectPoolConfiguration(
                prefab,
                name,
                prewarmCount,
                defaultCapacity,
                maxInactiveCount,
                collectionCheck,
                resetAttributedMembers,
                destroyedInstancePolicy,
                activeInstanceShutdownPolicy);
        }

        /// <inheritdoc />
        public override IPoolLifetimeConfiguration CreateLifetimeConfiguration()
        {
            return PoolLifetimeConfiguration.From(lifetime);
        }

        private void OnValidate()
        {
            defaultCapacity = Mathf.Max(1, defaultCapacity);
            maxInactiveCount = Mathf.Max(1, maxInactiveCount);
            prewarmCount = Mathf.Clamp(prewarmCount, 0, maxInactiveCount);
        }
    }
}
