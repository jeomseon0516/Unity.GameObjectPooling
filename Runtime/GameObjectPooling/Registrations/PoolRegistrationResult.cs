using System;
using Jeomseon.GameObjectPooling.Handles;

namespace Jeomseon.GameObjectPooling.Registrations
{
    /// <summary>
    /// Contains the terminal result of a callback-based asynchronous registration.
    /// Callback 기반 비동기 등록의 최종 결과를 보관합니다.
    /// </summary>
    public readonly struct PoolRegistrationResult
    {
        /// <summary>
        /// Gets the terminal registration status.
        /// 등록의 최종 상태를 가져옵니다.
        /// </summary>
        public PoolRegistrationStatus Status { get; }

        /// <summary>
        /// Gets the registered handle when the operation succeeded; otherwise null.
        /// 성공한 경우 등록된 Handle을 가져오며, 그 외에는 null입니다.
        /// </summary>
        public GameObjectPoolHandle Handle { get; }

        /// <summary>
        /// Gets the failure or cancellation exception; otherwise null.
        /// 실패 또는 취소 예외를 가져오며, 성공한 경우 null입니다.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets whether registration succeeded.
        /// 등록 성공 여부를 가져옵니다.
        /// </summary>
        public bool IsSucceeded => Status == PoolRegistrationStatus.Succeeded;

        /// <summary>
        /// Gets whether the caller canceled registration waiting.
        /// 호출자가 등록 대기를 취소했는지 가져옵니다.
        /// </summary>
        public bool IsCanceled => Status == PoolRegistrationStatus.Canceled;

        /// <summary>
        /// Gets whether creation or registration failed.
        /// 생성 또는 등록에 실패했는지 가져옵니다.
        /// </summary>
        public bool IsFailed => Status == PoolRegistrationStatus.Failed;

        private PoolRegistrationResult(
            PoolRegistrationStatus status,
            GameObjectPoolHandle handle,
            Exception exception)
        {
            Status = status;
            Handle = handle;
            Exception = exception;
        }

        internal static PoolRegistrationResult Success(GameObjectPoolHandle handle) =>
            new(PoolRegistrationStatus.Succeeded, handle, null);

        internal static PoolRegistrationResult Canceled(OperationCanceledException exception) =>
            new(PoolRegistrationStatus.Canceled, null, exception);

        internal static PoolRegistrationResult Failure(Exception exception) =>
            new(PoolRegistrationStatus.Failed, null, exception);
    }
}
