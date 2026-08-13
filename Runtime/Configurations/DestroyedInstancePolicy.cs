namespace Jeomseon.Unity.GameObjectPooling.Configurations
{
    /// <summary>
    /// Controls how a pool reacts when an instance was destroyed outside the pool.
    /// 인스턴스가 풀 외부에서 파괴됐을 때 풀의 대응 방식을 지정합니다.
    /// </summary>
    public enum DestroyedInstancePolicy
    {
        /// <summary>Throws immediately. 즉시 예외를 발생시킵니다.</summary>
        Throw,

        /// <summary>Skips destroyed entries and creates a replacement. 파괴된 항목을 건너뛰고 대체 객체를 생성합니다.</summary>
        Replace,

        /// <summary>Logs a warning and creates a replacement. 경고를 기록하고 대체 객체를 생성합니다.</summary>
        WarnAndReplace
    }
}
