using System;
using System.Text;
using Jeomseon.Pool;

namespace Jeomseon.Scope
{
    // TODO(lifecycle): Finalizer를 제거하고 명시적으로 Dispose하도록 변경해야 합니다.
    // Finalizer에서 인스턴스를 반환하면 풀의 Dispose 처리와 경합할 수 있습니다.
    public sealed class StringBuilderPoolScope : IDisposable
    {
        private readonly StringBuilderPool _selectPool;
        private readonly StringBuilder _stringBuilder;
        private bool _disposed;

        public StringBuilder Get()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StringBuilderPoolScope));
            return _stringBuilder;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_stringBuilder is not null)
            {
                _selectPool.Release(_stringBuilder);
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public StringBuilderPoolScope(StringBuilderPool selectPool = null)
        {
            _selectPool = selectPool ?? StringBuilderPool.Shared;
            _stringBuilder = _selectPool!.Get();
        }

        ~StringBuilderPoolScope()
        {
            Dispose();
        }
    }
}
