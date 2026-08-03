using System;
using System.Collections.Generic;
using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine.SceneManagement;

namespace Jeomseon.GameObjectPooling.Lifetimes
{
    /// <summary>
    /// Executes the built-in Scope, Scene, and Application lifetime policies.
    /// Scope, Scene 및 Application 기본 수명 정책을 실행합니다.
    /// </summary>
    internal sealed class BuiltInPoolLifetimeHandler : IPoolLifetimeHandler
    {
        private readonly GameObjectPoolScope _scope;
        private readonly bool _isPersistent;
        private readonly Dictionary<GameObjectPoolHandle, HashSet<int>> _sceneOwners = new();
        private PoolLifetimeRegistrationContext _context;
        private bool _hasContext;
        private bool _subscribed;

        internal BuiltInPoolLifetimeHandler(
            GameObjectPoolScope scope,
            bool isPersistent)
        {
            _scope = scope != null ? scope : throw new ArgumentNullException(nameof(scope));
            _isPersistent = isPersistent;
        }

        /// <inheritdoc />
        public bool CanHandle(IPoolLifetimeConfiguration configuration)
        {
            return configuration is PoolLifetimeConfiguration;
        }

        /// <inheritdoc />
        public void Validate(IPoolLifetimeConfiguration configuration)
        {
            if (configuration is not PoolLifetimeConfiguration builtIn)
            {
                throw new ArgumentException(
                    $"{GetType().Name} does not support {configuration?.GetType().FullName}.",
                    nameof(configuration));
            }

            if (builtIn.Lifetime != PoolLifetime.Application) return;
            if (!_isPersistent)
            {
                throw new InvalidOperationException(
                    "Application lifetime requires Dont Destroy On Load on " +
                    $"{nameof(GameObjectPoolScope)}. / Application 수명에는 Scope의 " +
                    "Dont Destroy On Load 설정이 필요합니다.");
            }

            if (_scope.transform.parent != null)
            {
                throw new InvalidOperationException(
                    $"Application lifetime requires {nameof(GameObjectPoolScope)} on a root " +
                    "GameObject. / Application 수명의 Scope는 루트 GameObject에 있어야 합니다.");
            }
        }

        /// <inheritdoc />
        public void Register(
            GameObjectPoolHandle handle,
            IPoolLifetimeConfiguration configuration,
            in PoolLifetimeRegistrationContext context)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            Validate(configuration);
            var builtIn = (PoolLifetimeConfiguration)configuration;
            if (builtIn.Lifetime != PoolLifetime.Scene) return;

            if (!_sceneOwners.TryGetValue(handle, out HashSet<int> owners))
            {
                owners = new HashSet<int>();
                _sceneOwners.Add(handle, owners);
            }

            owners.Add(context.OwnerSceneHandle);
            _context = context;
            _hasContext = true;
            Subscribe();
        }

        /// <inheritdoc />
        public void Unregister(GameObjectPoolHandle handle)
        {
            if (handle == null) return;
            _sceneOwners.Remove(handle);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_subscribed)
            {
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                _subscribed = false;
            }

            _sceneOwners.Clear();
            _hasContext = false;
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            _subscribed = true;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (!_hasContext) return;
            var handlesToRelease = new List<GameObjectPoolHandle>();
            foreach (KeyValuePair<GameObjectPoolHandle, HashSet<int>> pair in _sceneOwners)
            {
                pair.Value.Remove(scene.handle);
                if (pair.Value.Count == 0) handlesToRelease.Add(pair.Key);
            }

            foreach (GameObjectPoolHandle handle in handlesToRelease)
            {
                _context.Release(handle);
            }
        }
    }
}
