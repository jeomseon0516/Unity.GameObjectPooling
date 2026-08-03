namespace Jeomseon.GameObjectPooling.Registrations
{
    /// <summary>
    /// Describes the terminal state delivered by a callback-based pool registration.
    /// Callback 기반 Pool 등록이 전달하는 최종 상태를 나타냅니다.
    /// </summary>
    public enum PoolRegistrationStatus
    {
        /// <summary>
        /// The pool was registered successfully.
        /// Pool이 성공적으로 등록됐습니다.
        /// </summary>
        Succeeded,

        /// <summary>
        /// The caller canceled its wait for registration.
        /// 호출자가 Pool 등록 대기를 취소했습니다.
        /// </summary>
        Canceled,

        /// <summary>
        /// Pool creation or registration failed.
        /// Pool 생성 또는 등록에 실패했습니다.
        /// </summary>
        Failed
    }
}
