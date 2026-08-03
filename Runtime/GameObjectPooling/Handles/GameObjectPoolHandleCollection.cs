using System.Collections;
using System.Collections.Generic;

namespace Jeomseon.GameObjectPooling.Handles
{
    /// <summary>
    /// Exposes a live read-only view of scope-owned handles without exposing the mutable
    /// backing collection.
    /// 변경 가능한 내부 컬렉션을 노출하지 않고 Scope 소유 Handle의 실시간 읽기 전용 보기를
    /// 제공합니다.
    /// </summary>
    internal sealed class GameObjectPoolHandleCollection :
        IReadOnlyCollection<GameObjectPoolHandle>
    {
        private readonly IReadOnlyCollection<GameObjectPoolHandle> _handles;

        /// <inheritdoc />
        public int Count => _handles.Count;

        internal GameObjectPoolHandleCollection(
            IReadOnlyCollection<GameObjectPoolHandle> handles)
        {
            _handles = handles;
        }

        /// <inheritdoc />
        public IEnumerator<GameObjectPoolHandle> GetEnumerator()
        {
            return _handles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
