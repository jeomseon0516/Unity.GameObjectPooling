# GameObject Pooling 로드맵

우선순위: `P0` 결함·안전성 → `P1` 핵심 구조 → `P2` API·성능 → `P3` 장기 확장

## Unity 6000.6 호환성 점검 (2026-09-02)

- 최소 Unity 버전과 한·영 설치 문서를 `6000.6.0f1`로 상향했습니다.
- Unity 기본 `ObjectPool<T>`와 비교했을 때 이 패키지는 Definition 기반 구성, Scope 수명,
  Handle 무효화, 활성 인스턴스 종료 정책과 사용자 수명 처리기를 추가로 제공하므로 공식 API와
  의미상 중복되는 Wrapper가 아닙니다. 현재 제거 대상은 없습니다.
- Kilo 후속 보강을 포함한 Runtime·Editor·Sample 컴파일과 관련 EditMode/PlayMode 테스트를
  Unity 6000.6 Test Framework에서 다시 검증했습니다. EditMode 25/25, PlayMode 11/11이 통과했습니다.

## 테스트 모드 정리 (2026-08-18, Unity 검증 대기)

- `GameObjectPoolScope`의 `initializeOnAwake` 자동 초기화와 `OnDestroy` 자동 handle 무효화를 실제
  PlayMode 테스트로 추가했습니다.
- 기존 Editor 테스트는 Unity 메시지를 암묵적으로 기대하지 않고 명시적 `Initialize()` 계약을
  검증하도록 이름과 실행 경로를 수정했습니다.
- Runtime PlayMode 테스트의 private 직렬화 필드 reflection을 제거했습니다. 테스트 전용 Definition은
  공개 Configuration 계약으로 구성하고, 영속 Scope 조건은 실제 `DontDestroyOnLoad`로 만듭니다.

## 작업 순서

1. **완료 — P1-01 — ScriptableObject 풀 정의 도입**
   - 추상 Definition, 불변 런타임 Configuration, Unity 기본 Definition과 MonoBehaviour Scope를 추가했습니다.
   - Definition과 런타임 Configuration을 Registration으로 통합하고 Scope가 Handle을 런타임 식별자로 반환합니다.
   - Catalog는 여러 Definition의 일괄 등록 및 Preload 설정으로 제한합니다.
   - Definition은 Configuration 변환 경계로 제한하고 Factory와 런타임 풀의 ScriptableObject 의존성을 제거했습니다.
   - 풀 생성 설정과 수명 설정을 별도 Configuration으로 분리하고 런타임 생성 경로에서도 같은 수명 정책을 사용합니다.
   - Scope 설정으로 씬 수명과 전역 수명을 선택하고 Inspector에서 풀 목록을 관리합니다.
2. **완료 — P1-02 — 문자열 키 제거**
   - 문자열 키 저장소를 제거하고 Scope가 반환하는 Handle을 런타임 식별자로 사용합니다.
3. **완료 — P1-03 — 저장소 구현 통합**
   - 구 Generic/Keyed/GenericKeyed 정적 풀을 제거하고 Unity 기본 저장소와 공통 계약으로 통합했습니다.
4. **완료 — P2-01 — Attribute Reset 메타데이터 캐시**
   - `PoolReleaseResetter`가 Component 타입별 Field/Property 메타데이터를 한 번만 탐색합니다.
5. **완료 — P2-02 — 통계와 진단**
   - Capability 인터페이스로 활성·대기·생성·폐기·잘못된 반환·용량 초과 통계를 제공합니다.
   - PlayMode의 `GameObjectPoolScope` Inspector에서 Catalog 포함 여부와 관계없이 모든 등록 Handle의 통계를 표시합니다.
6. **완료 — P2-03 — 사용자 수명 처리기 확장**
   - 수명 정책 데이터를 `IPoolLifetimeConfiguration`, 실행 책임을 `IPoolLifetimeHandler`로 분리했습니다.
   - Scope가 사용자 Handler의 소유권과 Dispose를 관리하며 기본 Scope/Scene/Application Handler를 제공합니다.
7. **완료 — P2-04 — Owner 수명 정책**
   - 런타임 GameObject 또는 Component를 소유자로 지정하고 실제 파괴 시 풀을 해제합니다.
   - 비활성화와 파괴를 구분하며 소유자 하나로 여러 풀을 관리할 수 있습니다.
   - Scene 객체를 참조하는 Owner Configuration은 ScriptableObject Definition과 분리했습니다.
8. **완료 — P0-05 — Pool 종료 시 활성 인스턴스 정책** (2026-08-31)
   - `ActiveInstanceShutdownPolicy.Destroy`를 기본값으로 두어 기존처럼 Pool Scope와 활성
     인스턴스를 함께 파괴합니다.
   - `Preserve`는 Pool 종료 시 활성 인스턴스를 남기고 Pool 루트의 자식이면 먼저 분리합니다.
     이후 Handle은 무효이므로 Pool 반환은 불가능하며, 소유자가 남은 인스턴스를 직접 종료하고
     파괴해야 합니다.
   - `UnityGameObjectPoolDefinition` Inspector에서 Preserve 선택 시 위 책임을 한·영 HelpBox 경고로
     표시합니다. 기본 Destroy와 Preserve Dispose 동작을 Editor 테스트로 추가했습니다.
   - Unity Object의 파괴된 참조를 NUnit 참조 null로 비교하던 테스트 표현을 Unity의 `== null`
     규칙에 맞게 수정했고, 사용자가 GameObjectPooling Editor Test Runner 전체 통과를 확인했습니다.
   - 2026-09-02 Kilo 재검토에서 Preserve 부모 분리·Handle 무효화·비활성 인스턴스 파괴·외부 파괴
     안전성의 명시적 회귀 검증과 `PooledGameObjectState.Owner` 정리를 요청했습니다. 실제 코드와
     대조해 모두 타당하다고 판단했고, Preserve 시 State를 Pool에서 분리하며 관련 Editor 테스트를
     추가했습니다. Unity 6000.6 Test Framework에서 최신 추가 테스트를 포함해 EditMode 25/25,
     PlayMode 11/11이 통과했습니다.
