using System;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Contracts
{
    /// <summary>
    /// Defines the public contract for a GameObject pool. Consumers should depend on this
    /// interface so the Unity-backed implementation can be replaced by a custom pool.
    /// GameObject 풀의 공개 계약을 정의합니다. Unity 기반 구현을 사용자 정의 풀로 교체할 수
    /// 있도록 소비자는 이 인터페이스에 의존해야 합니다.
    /// </summary>
    public interface IGameObjectPool : IDisposable
    {
        /// <summary>
        /// Gets a GameObject from this pool.
        /// 이 Pool에서 GameObject를 가져옵니다.
        /// </summary>
        GameObject Get();

        /// <summary>
        /// Gets an instance and applies its spawn transform before activation.
        /// 인스턴스를 가져오고 활성화 전에 생성 Transform을 적용합니다.
        /// </summary>
        GameObject Get(in PoolSpawnOptions options);

        /// <summary>
        /// Returns a GameObject to this pool. This declaration keeps GameObject overload
        /// resolution unambiguous beside the generic Component overload.
        /// GameObject를 이 풀에 반환합니다. Generic Component 오버로드와 함께 사용할 때
        /// GameObject 오버로드가 명확하게 선택되도록 이 계약을 다시 선언합니다.
        /// </summary>
        void Release(GameObject instance);

        /// <summary>
        /// Gets a component from the pooled GameObject.
        /// 풀링된 GameObject에서 컴포넌트를 가져옵니다.
        /// </summary>
        T Get<T>() where T : Component;

        /// <summary>
        /// Gets a component after applying the supplied spawn transform.
        /// 지정된 생성 Transform을 적용한 다음 컴포넌트를 가져옵니다.
        /// </summary>
        T Get<T>(in PoolSpawnOptions options) where T : Component;

        /// <summary>
        /// Returns the GameObject that owns the supplied component.
        /// 지정된 컴포넌트를 소유한 GameObject를 반환합니다.
        /// </summary>
        void Release<T>(T component) where T : Component;
    }
}
