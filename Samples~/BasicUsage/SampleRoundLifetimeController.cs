using System;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Represents a project-owned round lifecycle source used by the custom lifetime sample.
    /// 사용자 수명 샘플에서 사용하는 프로젝트 소유 Round 수명 이벤트 소스를 나타냅니다.
    /// </summary>
    public sealed class SampleRoundLifetimeController : MonoBehaviour
    {
        /// <summary>
        /// Raised when a round ends.
        /// Round가 종료될 때 발생합니다.
        /// </summary>
        public event Action<int> RoundEnded;

        /// <summary>
        /// Ends a round and notifies registered lifetime handlers.
        /// Round를 종료하고 등록된 수명 Handler에 알립니다.
        /// </summary>
        public void EndRound(int roundId)
        {
            RoundEnded?.Invoke(roundId);
        }
    }
}
