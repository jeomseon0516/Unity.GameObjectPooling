using Jeomseon.GameObjectPooling.Contracts;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Providers;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Adapts IGameObjectPool to the project-facing actor Provider contract.
    /// IGameObjectPool을 프로젝트용 Actor Provider 계약으로 변환합니다.
    /// </summary>
    public sealed class SampleActorPoolProvider :
        ComponentPoolProvider<PooledSampleActor>,
        ISampleActorProvider
    {
        /// <summary>
        /// Creates a domain Provider backed by the supplied pool.
        /// 지정된 풀을 사용하는 도메인 Provider를 생성합니다.
        /// </summary>
        public SampleActorPoolProvider(GameObjectPoolHandle handle)
            : base(handle)
        {
        }

        /// <inheritdoc />
        public PooledSampleActor Spawn(
            string actorName,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            PooledSampleActor actor = Spawn(
                PoolSpawnOptions.At(position, rotation, parent));
            actor.Initialize(actorName);
            return actor;
        }

    }
}
