using Jeomseon.Unity.GameObjectPooling.Contracts;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Lifecycle
{
    [DisallowMultipleComponent]
    internal sealed class PooledGameObjectState : MonoBehaviour
    {
        internal IGameObjectPool Owner { get; private set; }
        internal IPoolGetHandler[] GetHandlers { get; private set; }
        internal IPoolReleaseHandler[] ReleaseHandlers { get; private set; }
        internal bool IsReleased { get; set; }

        internal void Initialize(IGameObjectPool owner)
        {
            Owner = owner;
            GetHandlers = GetComponents<IPoolGetHandler>();
            ReleaseHandlers = GetComponents<IPoolReleaseHandler>();
            IsReleased = true;
            hideFlags = HideFlags.HideInInspector;
        }
    }
}
