using Jeomseon.Scope;
using UnityEngine;

namespace Jeomseon.Samples.Pooling
{
    public sealed class PoolingSample : MonoBehaviour
    {
        [ContextMenu("List Pool 사용")]
        private void Run()
        {
            using ListPoolScope<int> scope = new();
            scope.Get().AddRange(new[] { 1, 2, 3 });
            Debug.Log($"풀링된 목록 항목 수: {scope.Get().Count}");
        }
    }
}
