using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Jeomseon.GameObjectPooling.Configurations
{
    /// <summary>
    /// Keeps a pool alive while a specific GameObject or Component owner exists. The owner
    /// reference is runtime-only and is intentionally separate from serialized Definitions.
    /// 특정 GameObject 또는 Component 소유자가 존재하는 동안 풀을 유지합니다. 소유자 참조는
    /// 런타임 전용이며 직렬화된 Definition과 의도적으로 분리됩니다.
    /// </summary>
    public sealed class OwnerPoolLifetimeConfiguration : IPoolLifetimeConfiguration
    {
        /// <summary>
        /// Gets the exact Unity object whose destruction ends the ownership.
        /// 파괴될 때 소유권을 종료하는 정확한 Unity 객체를 가져옵니다.
        /// </summary>
        public Object Owner { get; }

        /// <summary>
        /// Gets the GameObject containing the owner.
        /// 소유자를 포함하는 GameObject를 가져옵니다.
        /// </summary>
        public GameObject OwnerGameObject { get; }

        /// <summary>
        /// Creates an Owner lifetime for a GameObject or Component. Disabled owners remain
        /// valid; an owner that is already destroyed is rejected.
        /// GameObject 또는 Component의 Owner 수명을 생성합니다. 비활성 소유자는 계속 유효하며,
        /// 이미 파괴된 소유자는 거부합니다.
        /// </summary>
        public OwnerPoolLifetimeConfiguration(Object owner)
        {
            if (ReferenceEquals(owner, null))
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (owner == null)
            {
                throw new ArgumentException(
                    "The pool owner has already been destroyed. / 풀 소유자가 이미 파괴되었습니다.",
                    nameof(owner));
            }

            OwnerGameObject = owner switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => throw new ArgumentException(
                    "A pool owner must be a GameObject or Component. / 풀 소유자는 " +
                    "GameObject 또는 Component여야 합니다.",
                    nameof(owner))
            };
            Owner = owner;
        }
    }
}
