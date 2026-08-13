using System;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Lifetimes
{
    /// <summary>
    /// Provides a Unity player-loop tick to the Owner lifetime handler.
    /// Owner 수명 Handler에 Unity Player Loop Tick을 제공합니다.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class OwnerPoolLifetimeMonitor : MonoBehaviour
    {
        internal event Action Tick;

        private void LateUpdate() => Tick?.Invoke();
    }
}
