using System;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using Jeomseon.Unity.GameObjectPooling.Pools;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Factories
{
    /// <summary>
    /// Creates the default GameObject pool backed by UnityEngine.Pool.ObjectPool.
    /// UnityEngine.Pool.ObjectPool 기반 기본 GameObject 풀을 생성합니다.
    /// </summary>
    public sealed class UnityGameObjectPoolFactory : IGameObjectPoolFactory
    {
        /// <inheritdoc />
        public bool CanCreate(IGameObjectPoolConfiguration configuration)
        {
            return configuration is UnityGameObjectPoolConfiguration;
        }

        /// <inheritdoc />
        public IGameObjectPool Create(
            IGameObjectPoolConfiguration configuration,
            Transform runtimeRoot = null)
        {
            if (configuration is not UnityGameObjectPoolConfiguration unityConfiguration)
            {
                throw new ArgumentException(
                    $"{nameof(UnityGameObjectPoolFactory)} only supports " +
                    $"{nameof(UnityGameObjectPoolConfiguration)}.",
                    nameof(configuration));
            }

            return new UnityGameObjectPool(unityConfiguration, runtimeRoot);
        }
    }
}
