using System;

namespace Jeomseon.GameObjectPooling.Reset
{
    /// <summary>
    /// Marks an instance field or property for reset when its GameObject returns to a pool.
    /// GameObject가 풀에 반환될 때 복원할 인스턴스 필드 또는 프로퍼티를 표시합니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ResetOnPoolReleaseAttribute : Attribute
    {
        /// <summary>
        /// Gets the explicit reset value, or null to use the member type's default value.
        /// 명시적인 복원 값을 가져오며 null이면 멤버 타입의 기본값을 사용합니다.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Creates an attribute with an optional explicit reset value.
        /// 선택적인 명시적 복원 값을 사용하여 Attribute를 생성합니다.
        /// </summary>
        public ResetOnPoolReleaseAttribute(object value = null)
        {
            Value = value;
        }
    }
}
