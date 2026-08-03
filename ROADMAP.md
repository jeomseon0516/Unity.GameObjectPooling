# GameObject Pooling 로드맵

우선순위: `P0` 결함·안전성 → `P1` 핵심 구조 → `P2` API·성능 → `P3` 장기 확장

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
