using System;
using System.Buffers;
using UnityEngine;

namespace Jeomseon.Scope
{
    // TODO(lifecycle): Finalizer 기반 진단을 제거해야 합니다. Finalizer는 GC 스레드에서
    // 실행되므로 Unity API를 호출하면 안 됩니다. 에디터 진단이나 명시적인 소유권 검사를 사용합니다.
    public class ArrayPoolScope<T> : IDisposable
    {
        private readonly T[] _pooledArray;
        private bool _disposed;

        public T[] Get()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ArrayPoolScope<T>));

            return _pooledArray;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_pooledArray is not null)
            {
                ArrayPool<T>.Shared.Return(_pooledArray, true);
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public ArrayPoolScope(int minimumLength)
        {
            if (minimumLength <= 0) throw new ArgumentOutOfRangeException(nameof(minimumLength), "Minimum length must be greater than zero.");

            _pooledArray = ArrayPool<T>.Shared.Rent(minimumLength);
        }

#if DEBUG
        ~ArrayPoolScope()
        {
            if (!_disposed)
            {
                Debug.LogWarning($"{nameof(ArrayPoolScope<T>)} was not disposed properly.");
                Dispose();
            }
        }
#endif
    }
}
