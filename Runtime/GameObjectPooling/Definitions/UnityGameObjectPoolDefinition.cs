using Jeomseon.GameObjectPooling.Configurations;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Definitions
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
        [SerializeField] private GameObject _prefab;
        [SerializeField, Min(0)] private int _prewarmCount;
        [SerializeField, Min(1)] private int _defaultCapacity = 10;
        [SerializeField, Min(1)] private int _maxInactiveCount = 100;
        [SerializeField] private bool _collectionCheck = true;
        [SerializeField] private bool _resetAttributedMembers = true;
        [SerializeField] private DestroyedInstancePolicy _destroyedInstancePolicy =
            DestroyedInstancePolicy.WarnAndReplace;
        [SerializeField] private PoolLifetime _lifetime = PoolLifetime.Scope;

        /// <summary>
        /// Gets the prefab instantiated when the pool is empty.
        /// 풀이 비어 있을 때 생성할 Prefab을 가져옵니다.
        /// </summary>
        public GameObject Prefab => _prefab;

        /// <summary>
        /// Gets the number of inactive instances created with the pool.
        /// 풀 생성 시 미리 생성할 비활성 인스턴스 개수를 가져옵니다.
        /// </summary>
        public int PrewarmCount => _prewarmCount;

        /// <summary>
        /// Gets Unity ObjectPool's initial internal collection capacity.
        /// Unity ObjectPool 내부 컬렉션의 초기 용량을 가져옵니다.
        /// </summary>
        public int DefaultCapacity => _defaultCapacity;

        /// <summary>
        /// Gets the maximum inactive count. This is not a limit on active instances.
        /// 최대 비활성 보관 개수를 가져옵니다. 활성 인스턴스의 최대 개수는 아닙니다.
        /// </summary>
        public int MaxInactiveCount => _maxInactiveCount;

        /// <summary>
        /// Gets whether Unity's duplicate-release validation is enabled.
        /// Unity의 중복 반환 검사가 활성화되었는지 가져옵니다.
        /// </summary>
        public bool CollectionCheck => _collectionCheck;

        /// <summary>
        /// Gets whether ResetOnPoolReleaseAttribute members are restored on release.
        /// 반환 시 ResetOnPoolReleaseAttribute 멤버를 복원할지 가져옵니다.
        /// </summary>
        public bool ResetAttributedMembers => _resetAttributedMembers;

        /// <summary>
        /// Gets how externally destroyed instances are handled.
        /// 외부에서 파괴된 인스턴스를 처리하는 방식을 가져옵니다.
        /// </summary>
        public DestroyedInstancePolicy DestroyedInstancePolicy => _destroyedInstancePolicy;

        /// <inheritdoc />
        public override IGameObjectPoolConfiguration CreateConfiguration()
        {
            return new UnityGameObjectPoolConfiguration(
                _prefab,
                name,
                _prewarmCount,
                _defaultCapacity,
                _maxInactiveCount,
                _collectionCheck,
                _resetAttributedMembers,
                _destroyedInstancePolicy);
        }

        /// <inheritdoc />
        public override IPoolLifetimeConfiguration CreateLifetimeConfiguration()
        {
            return PoolLifetimeConfiguration.From(_lifetime);
        }

        private void OnValidate()
        {
            _defaultCapacity = Mathf.Max(1, _defaultCapacity);
            _maxInactiveCount = Mathf.Max(1, _maxInactiveCount);
            _prewarmCount = Mathf.Clamp(_prewarmCount, 0, _maxInactiveCount);
        }
    }
}
