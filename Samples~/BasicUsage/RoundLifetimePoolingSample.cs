using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Composes a runtime pool with a project-defined round lifetime handler.
    /// 프로젝트 정의 Round 수명 Handler로 런타임 풀을 구성합니다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class RoundLifetimePoolingSample : MonoBehaviour
    {
        [SerializeField] private GameObjectPoolScope _scope;
        [SerializeField] private SampleRoundLifetimeController _roundController;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _roundId = 1;

        private GameObjectPoolHandle _handle;
        private GameObject _instance;

        private void Awake()
        {
            if (_scope == null || _roundController == null || _prefab == null) return;

            _scope.RegisterLifetimeHandler(
                new RoundPoolLifetimeHandler(_roundController));
            _handle = _scope.Register(
                new UnityGameObjectPoolConfiguration(_prefab, "Round Lifetime Pool", 1),
                new RoundLifetimeConfiguration(_roundId));
            _scope.SetDefault(_handle);
        }

        [ContextMenu("Spawn Round Object / Round 객체 생성")]
        private void Spawn()
        {
            if (_instance != null || _handle == null || !_handle.IsValid) return;
            _instance = _handle.Spawn();
        }

        [ContextMenu("End Round And Release Pool / Round 종료 및 풀 해제")]
        private void EndRound()
        {
            if (_roundController == null) return;

            _roundController.EndRound(_roundId);
            _instance = null;
            Debug.Log(
                _handle != null && !_handle.IsValid
                    ? "[PASS] Round pool was released. / [통과] Round 풀이 해제됐습니다."
                    : "[FAIL] Round pool is still valid. / [실패] Round 풀이 아직 유효합니다.",
                this);
        }
    }
}
