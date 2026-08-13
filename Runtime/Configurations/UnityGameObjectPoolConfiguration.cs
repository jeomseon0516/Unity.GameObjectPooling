using System;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Configurations
{
    /// <summary>
    /// Contains immutable construction settings for the Unity ObjectPool-backed GameObject
    /// pool. It can be created directly at runtime or produced by a serialized definition.
    /// Unity ObjectPool 기반 GameObject 풀의 불변 생성 설정을 보관합니다. 런타임 코드에서
    /// 직접 만들거나 직렬화된 Definition을 통해 생성할 수 있습니다.
    /// </summary>
    public sealed class UnityGameObjectPoolConfiguration : IGameObjectPoolConfiguration
    {
        /// <summary>
        /// Gets the diagnostic name used for the runtime pool root and log messages.
        /// 런타임 풀 루트와 로그 메시지에 사용할 진단 이름을 가져옵니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the prefab instantiated when the pool is empty.
        /// 풀이 비어 있을 때 생성할 Prefab을 가져옵니다.
        /// </summary>
        public GameObject Prefab { get; }

        /// <summary>
        /// Gets the number of inactive instances created with the pool.
        /// 풀 생성 시 미리 생성할 비활성 인스턴스 개수를 가져옵니다.
        /// </summary>
        public int PrewarmCount { get; }

        /// <summary>
        /// Gets Unity ObjectPool's initial internal collection capacity.
        /// Unity ObjectPool 내부 컬렉션의 초기 용량을 가져옵니다.
        /// </summary>
        public int DefaultCapacity { get; }

        /// <summary>
        /// Gets the maximum number of inactive instances retained by the pool.
        /// 풀이 보관하는 최대 비활성 인스턴스 개수를 가져옵니다.
        /// </summary>
        public int MaxInactiveCount { get; }

        /// <summary>
        /// Gets whether Unity's duplicate-release validation is enabled.
        /// Unity의 중복 반환 검사가 활성화되었는지 가져옵니다.
        /// </summary>
        public bool CollectionCheck { get; }

        /// <summary>
        /// Gets whether ResetOnPoolReleaseAttribute members are restored on release.
        /// 반환 시 ResetOnPoolReleaseAttribute 멤버를 복원할지 가져옵니다.
        /// </summary>
        public bool ResetAttributedMembers { get; }

        /// <summary>
        /// Gets how externally destroyed instances are handled.
        /// 외부에서 파괴된 인스턴스를 처리하는 방식을 가져옵니다.
        /// </summary>
        public DestroyedInstancePolicy DestroyedInstancePolicy { get; }

        /// <summary>
        /// Creates validated runtime settings for a Unity-backed GameObject pool.
        /// Unity 기반 GameObject 풀을 위한 검증된 런타임 설정을 생성합니다.
        /// </summary>
        public UnityGameObjectPoolConfiguration(
            GameObject prefab,
            string name = null,
            int prewarmCount = 0,
            int defaultCapacity = 10,
            int maxInactiveCount = 100,
            bool collectionCheck = true,
            bool resetAttributedMembers = true,
            DestroyedInstancePolicy destroyedInstancePolicy =
                DestroyedInstancePolicy.WarnAndReplace)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (defaultCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultCapacity));
            }

            if (maxInactiveCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxInactiveCount));
            }

            if (prewarmCount < 0 || prewarmCount > maxInactiveCount)
            {
                throw new ArgumentOutOfRangeException(nameof(prewarmCount));
            }

            Prefab = prefab;
            Name = string.IsNullOrWhiteSpace(name) ? prefab.name : name;
            PrewarmCount = prewarmCount;
            DefaultCapacity = defaultCapacity;
            MaxInactiveCount = maxInactiveCount;
            CollectionCheck = collectionCheck;
            ResetAttributedMembers = resetAttributedMembers;
            DestroyedInstancePolicy = destroyedInstancePolicy;
        }
    }
}
