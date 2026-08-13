using System;
using System.Collections.Generic;
using System.Threading;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using Jeomseon.Unity.GameObjectPooling.Factories;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Services
{
    /// <summary>
    /// Default pool manager. It resolves definitions through registered factories and stores
    /// pools only through IGameObjectPool, allowing custom implementations to replace the
    /// Unity-backed default without changing consumers.
    /// 기본 풀 관리자입니다. 등록된 Factory를 통해 Definition을 해석하고 풀을
    /// IGameObjectPool로만 보관하여 소비자를 변경하지 않고 Unity 기본 구현을 교체할 수 있습니다.
    /// </summary>
    public sealed class GameObjectPoolService : IGameObjectPoolService
    {
        private readonly HashSet<IGameObjectPool> _ownedPools = new();
        private readonly List<IGameObjectPoolFactory> _factories = new();
        private readonly List<IAsyncGameObjectPoolFactory> _asyncFactories = new();
        private readonly Transform _runtimeRoot;
        private bool _disposed;

        /// <summary>
        /// Creates a service and registers UnityGameObjectPoolFactory as its fallback factory.
        /// Custom factories registered later take precedence over earlier factories.
        /// 서비스를 생성하고 UnityGameObjectPoolFactory를 fallback Factory로 등록합니다.
        /// 나중에 등록된 사용자 Factory가 이전 Factory보다 우선합니다.
        /// </summary>
        public GameObjectPoolService(Transform runtimeRoot = null)
        {
            _runtimeRoot = runtimeRoot;
            _factories.Add(new UnityGameObjectPoolFactory());
        }

        /// <inheritdoc />
        public IGameObjectPool CreatePool(IGameObjectPoolConfiguration configuration)
        {
            ThrowIfDisposed();
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return CreatePoolInternal(configuration);
        }

        /// <inheritdoc />
        public async Awaitable<IGameObjectPool> CreatePoolAsync(
            IGameObjectPoolConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            cancellationToken.ThrowIfCancellationRequested();
            for (int i = _asyncFactories.Count - 1; i >= 0; i--)
            {
                IAsyncGameObjectPoolFactory factory = _asyncFactories[i];
                if (!factory.CanCreate(configuration)) continue;

                IGameObjectPool pool = await factory.CreateAsync(
                    configuration,
                    _runtimeRoot,
                    cancellationToken);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Awaitable.MainThreadAsync();
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch
                {
                    pool?.Dispose();
                    throw;
                }

                return OwnPool(factory, pool);
            }

            // A synchronous factory is already complete and is therefore a safe fallback.
            // 동기 Factory는 이미 완료된 작업이므로 안전한 fallback입니다.
            IGameObjectPool fallbackPool = CreatePoolInternal(configuration);
            if (cancellationToken.IsCancellationRequested)
            {
                ReleaseOwnedPool(fallbackPool);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return fallbackPool;
        }

        /// <inheritdoc />
        public bool ReleasePool(IGameObjectPool pool)
        {
            ThrowIfDisposed();
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool));
            }

            if (!_ownedPools.Contains(pool)) return false;

            return ReleaseOwnedPool(pool);
        }

        /// <inheritdoc />
        public void RegisterFactory(IGameObjectPoolFactory factory)
        {
            ThrowIfDisposed();
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _factories.Add(factory);
        }

        /// <inheritdoc />
        public void RegisterAsyncFactory(IAsyncGameObjectPoolFactory factory)
        {
            ThrowIfDisposed();
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _asyncFactories.Add(factory);
        }

        /// <summary>
        /// Disposes every pool created and owned by this service.
        /// 이 서비스가 생성하고 소유한 모든 풀을 해제합니다.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            foreach (IGameObjectPool pool in _ownedPools)
            {
                pool.Dispose();
            }

            _ownedPools.Clear();
            _factories.Clear();
            _asyncFactories.Clear();
        }

        private IGameObjectPool CreatePoolInternal(
            IGameObjectPoolConfiguration configuration)
        {
            for (int i = _factories.Count - 1; i >= 0; i--)
            {
                IGameObjectPoolFactory factory = _factories[i];
                if (!factory.CanCreate(configuration)) continue;

                IGameObjectPool pool = factory.Create(configuration, _runtimeRoot);
                return OwnPool(factory, pool);
            }

            throw new InvalidOperationException(
                $"No pool factory supports {configuration.GetType().FullName}.");
        }

        private IGameObjectPool OwnPool(object factory, IGameObjectPool pool)
        {
            if (pool == null)
            {
                throw new InvalidOperationException(
                    $"{factory.GetType().FullName} returned a null pool.");
            }

            if (_disposed)
            {
                pool.Dispose();
                throw new ObjectDisposedException(nameof(GameObjectPoolService));
            }

            if (!_ownedPools.Add(pool))
            {
                throw new InvalidOperationException(
                    $"{factory.GetType().FullName} returned a pool already owned by this service.");
            }

            return pool;
        }

        private bool ReleaseOwnedPool(IGameObjectPool pool)
        {
            if (!_ownedPools.Remove(pool)) return false;

            pool.Dispose();
            return true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GameObjectPoolService));
            }
        }
    }
}
