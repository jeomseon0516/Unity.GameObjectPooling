using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Jeomseon.GameObjectPooling.Lifetimes
{
    /// <summary>
    /// Releases Owner-lifetime pools after their last registered owner is destroyed.
    /// Disabled owners remain valid. Component owners are tracked independently from their
    /// GameObject, so destroying only that Component also ends its ownership.
    /// 마지막으로 등록된 소유자가 파괴되면 Owner 수명 풀을 해제합니다. 비활성 소유자는 계속
    /// 유효합니다. Component 소유자는 GameObject와 독립적으로 추적하므로 해당 Component만
    /// 파괴해도 소유권이 종료됩니다.
    /// </summary>
    internal sealed class OwnerPoolLifetimeHandler : IPoolLifetimeHandler
    {
        private sealed class OwnerReferenceComparer : IEqualityComparer<Object>
        {
            internal static OwnerReferenceComparer Instance { get; } = new();

            public bool Equals(Object x, Object y) => ReferenceEquals(x, y);

            public int GetHashCode(Object value) => RuntimeHelpers.GetHashCode(value);
        }

        private readonly GameObjectPoolScope _scope;
        private readonly Dictionary<Object, HashSet<GameObjectPoolHandle>> _handlesByOwner =
            new(OwnerReferenceComparer.Instance);
        private readonly Dictionary<GameObjectPoolHandle, HashSet<Object>> _ownersByHandle =
            new();
        private readonly Dictionary<GameObjectPoolHandle, PoolLifetimeRegistrationContext>
            _contexts = new();
        private readonly List<Object> _destroyedOwners = new();
        private readonly List<GameObjectPoolHandle> _handlesToRelease = new();
        private OwnerPoolLifetimeMonitor _monitor;
        private bool _disposed;

        internal OwnerPoolLifetimeHandler(GameObjectPoolScope scope)
        {
            _scope = scope != null ? scope : throw new ArgumentNullException(nameof(scope));
        }

        /// <inheritdoc />
        public bool CanHandle(IPoolLifetimeConfiguration configuration) =>
            configuration is OwnerPoolLifetimeConfiguration;

        /// <inheritdoc />
        public void Validate(IPoolLifetimeConfiguration configuration)
        {
            if (configuration is not OwnerPoolLifetimeConfiguration ownerLifetime)
            {
                throw new ArgumentException(
                    $"{GetType().Name} does not support {configuration?.GetType().FullName}.",
                    nameof(configuration));
            }

            if (ownerLifetime.Owner == null)
            {
                throw new InvalidOperationException(
                    "The pool owner has already been destroyed. / 풀 소유자가 이미 파괴되었습니다.");
            }
        }

        /// <inheritdoc />
        public void Register(
            GameObjectPoolHandle handle,
            IPoolLifetimeConfiguration configuration,
            in PoolLifetimeRegistrationContext context)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            Validate(configuration);
            Object owner = ((OwnerPoolLifetimeConfiguration)configuration).Owner;

            if (!_ownersByHandle.TryGetValue(handle, out HashSet<Object> owners))
            {
                owners = new HashSet<Object>(OwnerReferenceComparer.Instance);
                _ownersByHandle.Add(handle, owners);
            }

            if (!owners.Add(owner)) return;
            if (!_handlesByOwner.TryGetValue(owner, out HashSet<GameObjectPoolHandle> handles))
            {
                handles = new HashSet<GameObjectPoolHandle>();
                _handlesByOwner.Add(owner, handles);
            }

            handles.Add(handle);
            _contexts[handle] = context;
            EnsureMonitor();
        }

        /// <inheritdoc />
        public void Unregister(GameObjectPoolHandle handle)
        {
            if (handle == null || !_ownersByHandle.Remove(handle, out HashSet<Object> owners))
            {
                return;
            }

            foreach (Object owner in owners)
            {
                if (!_handlesByOwner.TryGetValue(owner, out HashSet<GameObjectPoolHandle> handles))
                {
                    continue;
                }

                handles.Remove(handle);
                if (handles.Count == 0) _handlesByOwner.Remove(owner);
            }

            _contexts.Remove(handle);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            if (_monitor != null) _monitor.Tick -= PollDestroyedOwners;
            _handlesByOwner.Clear();
            _ownersByHandle.Clear();
            _contexts.Clear();
            _destroyedOwners.Clear();
            _handlesToRelease.Clear();
            _disposed = true;
        }

        private void EnsureMonitor()
        {
            if (_monitor != null) return;
            _monitor = _scope.GetComponent<OwnerPoolLifetimeMonitor>();
            if (_monitor == null)
            {
                _monitor = _scope.gameObject.AddComponent<OwnerPoolLifetimeMonitor>();
                _monitor.hideFlags = HideFlags.HideInInspector;
            }

            _monitor.Tick += PollDestroyedOwners;
        }

        private void PollDestroyedOwners()
        {
            _destroyedOwners.Clear();
            foreach (Object owner in _handlesByOwner.Keys)
            {
                if (owner == null) _destroyedOwners.Add(owner);
            }

            foreach (Object owner in _destroyedOwners) RemoveDestroyedOwner(owner);
            _destroyedOwners.Clear();
        }

        private void RemoveDestroyedOwner(Object owner)
        {
            if (!_handlesByOwner.Remove(owner, out HashSet<GameObjectPoolHandle> handles)) return;
            _handlesToRelease.Clear();
            foreach (GameObjectPoolHandle handle in handles)
            {
                if (!_ownersByHandle.TryGetValue(handle, out HashSet<Object> owners)) continue;
                owners.Remove(owner);
                if (owners.Count != 0) continue;
                _ownersByHandle.Remove(handle);
                _handlesToRelease.Add(handle);
            }

            foreach (GameObjectPoolHandle handle in _handlesToRelease)
            {
                if (!_contexts.Remove(handle, out PoolLifetimeRegistrationContext context)) continue;
                context.Release(handle);
            }

            _handlesToRelease.Clear();
        }
    }
}
