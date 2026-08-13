using System;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using Jeomseon.Unity.GameObjectPooling.Factories;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Selects CountingPoolConfiguration and decorates Unity's default pool.
    /// CountingPoolConfiguration을 선택하고 Unity 기본 풀을 Decorator로 감쌉니다.
    /// </summary>
    public sealed class CountingGameObjectPoolFactory : IGameObjectPoolFactory
    {
        private readonly UnityGameObjectPoolFactory _unityFactory = new();

        /// <inheritdoc />
        public bool CanCreate(IGameObjectPoolConfiguration configuration)
        {
            return configuration is CountingPoolConfiguration;
        }

        /// <inheritdoc />
        public IGameObjectPool Create(
            IGameObjectPoolConfiguration configuration,
            Transform runtimeRoot = null)
        {
            if (configuration is not CountingPoolConfiguration countingConfiguration)
            {
                throw new ArgumentException(
                    $"{nameof(CountingGameObjectPoolFactory)} requires " +
                    $"{nameof(CountingPoolConfiguration)}.",
                    nameof(configuration));
            }

            IGameObjectPool innerPool = _unityFactory.Create(
                countingConfiguration.InnerConfiguration,
                runtimeRoot);
            return new CountingGameObjectPool(innerPool);
        }
    }
}
