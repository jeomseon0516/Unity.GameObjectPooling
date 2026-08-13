using System;
using System.Collections.Generic;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Handles;
using Jeomseon.Unity.GameObjectPooling.Lifetimes;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Releases registered pools when their project round ends.
    /// 등록된 풀의 프로젝트 Round가 종료되면 해당 풀을 해제합니다.
    /// </summary>
    public sealed class RoundPoolLifetimeHandler : IPoolLifetimeHandler
    {
        private readonly SampleRoundLifetimeController _controller;
        private readonly Dictionary<GameObjectPoolHandle, Registration> _registrations = new();

        /// <summary>
        /// Creates a handler and observes the supplied project lifecycle source.
        /// Handler를 생성하고 지정된 프로젝트 수명 이벤트 소스를 관찰합니다.
        /// </summary>
        public RoundPoolLifetimeHandler(SampleRoundLifetimeController controller)
        {
            _controller = controller != null
                ? controller
                : throw new ArgumentNullException(nameof(controller));
            _controller.RoundEnded += OnRoundEnded;
        }

        /// <inheritdoc />
        public bool CanHandle(IPoolLifetimeConfiguration configuration)
        {
            return configuration is RoundLifetimeConfiguration;
        }

        /// <inheritdoc />
        public void Validate(IPoolLifetimeConfiguration configuration)
        {
            if (!CanHandle(configuration))
            {
                throw new ArgumentException(
                    $"{GetType().Name} requires {nameof(RoundLifetimeConfiguration)}.",
                    nameof(configuration));
            }
        }

        /// <inheritdoc />
        public void Register(
            GameObjectPoolHandle handle,
            IPoolLifetimeConfiguration configuration,
            in PoolLifetimeRegistrationContext context)
        {
            Validate(configuration);
            int roundId = ((RoundLifetimeConfiguration)configuration).RoundId;
            _registrations[handle] = new Registration(roundId, context);
        }

        /// <inheritdoc />
        public void Unregister(GameObjectPoolHandle handle)
        {
            if (handle != null) _registrations.Remove(handle);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _controller.RoundEnded -= OnRoundEnded;
            _registrations.Clear();
        }

        private void OnRoundEnded(int roundId)
        {
            var expiredHandles = new List<GameObjectPoolHandle>();
            foreach (KeyValuePair<GameObjectPoolHandle, Registration> pair in _registrations)
            {
                if (pair.Value.RoundId == roundId) expiredHandles.Add(pair.Key);
            }

            foreach (GameObjectPoolHandle handle in expiredHandles)
            {
                if (!_registrations.TryGetValue(handle, out Registration registration)) continue;
                registration.Context.Release(handle);
            }
        }

        private readonly struct Registration
        {
            internal int RoundId { get; }
            internal PoolLifetimeRegistrationContext Context { get; }

            internal Registration(
                int roundId,
                in PoolLifetimeRegistrationContext context)
            {
                RoundId = roundId;
                Context = context;
            }
        }
    }
}
