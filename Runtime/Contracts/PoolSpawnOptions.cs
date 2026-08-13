using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Contracts
{
    /// <summary>
    /// Describes the transform applied before a pooled GameObject is activated.
    /// 풀링된 GameObject가 활성화되기 전에 적용할 Transform을 정의합니다.
    /// </summary>
    public readonly struct PoolSpawnOptions
    {
        /// <summary>
        /// Gets whether an explicit world-space pose was supplied.
        /// 명시적인 월드 공간 Pose가 지정되었는지 가져옵니다.
        /// </summary>
        public bool HasPose { get; }

        /// <summary>
        /// Gets the optional parent assigned before activation.
        /// 활성화 전에 지정할 선택적 부모를 가져옵니다.
        /// </summary>
        public Transform Parent { get; }

        /// <summary>
        /// Gets the requested world-space position.
        /// 요청된 월드 공간 위치를 가져옵니다.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Gets the requested world-space rotation.
        /// 요청된 월드 공간 회전을 가져옵니다.
        /// </summary>
        public Quaternion Rotation { get; }

        private PoolSpawnOptions(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            bool hasPose)
        {
            Parent = parent;
            Position = position;
            Rotation = rotation;
            HasPose = hasPose;
        }

        /// <summary>
        /// Creates options that only assign a parent.
        /// 부모만 지정하는 옵션을 생성합니다.
        /// </summary>
        public static PoolSpawnOptions Under(Transform parent)
        {
            return new PoolSpawnOptions(parent, default, default, false);
        }

        /// <summary>
        /// Creates options with a world-space pose and an optional parent.
        /// 월드 공간 Pose와 선택적 부모를 지정하는 옵션을 생성합니다.
        /// </summary>
        public static PoolSpawnOptions At(
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            return new PoolSpawnOptions(parent, position, rotation, true);
        }
    }
}
