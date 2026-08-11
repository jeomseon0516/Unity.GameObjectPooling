using System.Collections;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.GameObjectPooling
{
    /// <summary>
    /// Provides a manual Play Mode check for an Application-lifetime pool. Run the context
    /// menu command while playing; it unloads the owner scene and verifies that the persistent
    /// scope, handle, and pooled object remain usable.
    /// Application 수명 풀의 수동 Play Mode 검증 환경을 제공합니다. 재생 중 Context Menu를
    /// 실행하면 소유 Scene을 언로드하고 영속 Scope, Handle 및 풀 객체가 계속 사용 가능한지
    /// 확인합니다.
    /// </summary>
    public sealed class ApplicationLifetimePoolingSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_scope")] private GameObjectPoolScope scope;
        private bool _isRunning;

        [ContextMenu("Run Application Lifetime Check / Application 수명 검사 실행")]
        private void RunApplicationLifetimeCheck()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "Run this check in Play Mode. / 이 검사는 Play Mode에서 실행하세요.",
                    this);
                return;
            }

            if (_isRunning || scope == null) return;
            StartCoroutine(VerifyAfterOwnerSceneUnload());
        }

        private IEnumerator VerifyAfterOwnerSceneUnload()
        {
            _isRunning = true;
            Scene ownerScene = SceneManager.GetActiveScene();
            GameObjectPoolHandle handle = scope.DefaultHandle;

            GameObject beforeUnload = handle.Spawn();
            handle.Despawn(beforeUnload);

            Scene verificationScene = SceneManager.CreateScene(
                "Pooling Application Lifetime Verification");
            SceneManager.SetActiveScene(verificationScene);
            yield return SceneManager.UnloadSceneAsync(ownerScene);

            if (scope == null || !handle.IsValid)
            {
                Debug.LogError(
                    "[FAIL] Application pool did not survive the owner scene unload. / " +
                    "[실패] Application 풀이 소유 Scene 언로드 후 유지되지 않았습니다.");
                _isRunning = false;
                yield break;
            }

            GameObject afterUnload = handle.Spawn();
            handle.Despawn(afterUnload);
            Debug.Log(
                "[PASS] Application scope and pool survived the owner scene unload. / " +
                "[통과] Application Scope와 풀이 소유 Scene 언로드 후에도 유지됐습니다.",
                this);
            _isRunning = false;
        }
    }
}
