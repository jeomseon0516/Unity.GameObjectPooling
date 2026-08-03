# 변경 기록

## [Unreleased]

## [0.2.0] - 2026-08-03

### Added

- 동기 API를 유지하면서 `Awaitable` 기반 풀 생성, Scope 등록, 초기화 및 Catalog preload API를 추가했습니다.
- 비동기 Factory 우선 선택과 동기 Factory fallback을 추가했습니다.
- 같은 Definition의 동시 비동기 등록을 하나의 생성 작업으로 병합했습니다.
- 취소 rollback, 비동기 PlayMode 검증 및 고수준 호출 샘플을 추가했습니다.
- 성공·취소·실패를 `PoolRegistrationResult`로 전달하는 Callback 편의 API를 추가했습니다.

### Documentation

- Definition, Catalog, Scope, Registration, Handle, 사용자 Factory, Variant Provider 및 수명 정책 사용법을 문서화했습니다.
- Definition, 런타임 Configuration, 프로젝트 Provider 접근 및 Application 수명 검증을 포함하는 샘플 씬을 추가했습니다.

### Changed

- 패키지 정체성을 범용 Pooling에서 GameObject와 Component 전용 `GameObjectPooling`으로 전환했습니다.
- `IGameObjectPool`이 대여·반환 계약을 직접 선언하도록 하고 사용처가 없던 `IPool<T>`를 제거했습니다.
- GameObject Pooling과 무관한 `StringBuilderPool` 및 테스트를 `Jeomseon.Unity.Core`로 이동했습니다.
- ScriptableObject Definition을 불변 런타임 Configuration으로 변환하는 경계를 추가하고 Factory와 Pool의 Definition 의존성을 제거했습니다.
- 수명 설정을 `IPoolLifetimeConfiguration`으로 분리하고 Definition 없는 런타임 풀에도 Scope/Scene/Application 수명을 적용할 수 있게 했습니다.
- `GameObjectPoolService`에서 Definition 없이 Configuration으로 풀을 생성하고 개별 해제할 수 있게 했습니다.
- Definition과 런타임 Configuration을 `GameObjectPoolRegistration`으로 통합하고 Scope가 반환하는 `GameObjectPoolHandle`을 런타임 식별자로 분리했습니다.
- Definition은 Scope 안에서 공유 Handle로 조회하고 런타임 Registration은 기본적으로 독립 Handle을 생성하도록 명확히 구분했습니다.
- 프로젝트별 `IPoolLifetimeConfiguration`을 실행하는 `IPoolLifetimeHandler` 확장 지점과 Scope 소유 수명 처리기 관리를 추가했습니다.
- `ComponentPoolProvider<T>`가 Scope 소유 Handle만 받도록 직접 `IGameObjectPool` 생성자를 제거했습니다.
- Round 종료 이벤트로 풀을 해제하는 사용자 수명 Handler와 구성 완료 샘플을 추가했습니다.
- Scope의 Handle 진단 컬렉션을 변경 불가능한 실시간 View로 보호했습니다.
- 저수준 Configuration/Factory/Pool Decorator부터 고수준 도메인 Provider까지 단계별 샘플을 추가했습니다.
- `StringBuilderPool`을 Unity `IObjectPool<T>` 계약과 `PooledObject<T>` 기반 `using` 패턴에 맞췄습니다.
- `StringBuilderPool`의 블로킹 동기화를 제거하고 보관 한도, 최대 용량 및 중복 반환 검사를 추가했습니다.
- `IGameObjectPool`과 Factory 기반 관리 계약을 추가하고, 기본 구현을 Unity `ObjectPool<GameObject>` 위에 구성했습니다.
- Component 기반 사용성을 별도 저장소 없이 확장하는 `ComponentPoolProvider<T>`를 추가했습니다.
- `PoolInitAttribute`를 실제 반환 시점을 나타내는 `ResetOnPoolReleaseAttribute`로 변경했습니다.
- Definition별 외부 파괴 대응 및 Scope/Scene/Application 수명 정책을 추가했습니다.
- Clear, Prewarm, Diagnostics Capability 계약과 Catalog 외 런타임 등록까지 포함하는 Scope 통계를 추가했습니다.

### Removed

- .NET과 Unity 기본 풀을 단순히 감싸던 Array/List/Dictionary/StringBuilder Scope 계층을 제거했습니다.
- 중복 저장소를 소유하던 `GenericObjectPool`, `KeyedObjectPool`, `GenericKeyedObjectPool`을 제거했습니다.
- Definition을 런타임 저장소 키로 사용하던 Service API와 `IGameObjectPoolSource` 계층을 제거했습니다.
- 구 `ObjectPool` 및 `Pool` 디렉터리를 제거하고 런타임 코드를 `Pooling` 책임 영역으로 통합했습니다.

## [0.1.2] - 2026-07-29

- asmdef의 `rootNamespace`와 Pool·ObjectPool·Scope 파일 위치를 namespace에 맞게 정리했습니다.

## [0.1.1] - 2026-07-29

- `ListPoolScope<T>`의 대여·반환 흐름을 확인하는 `Basic Usage` 샘플을 추가했습니다.

이 패키지의 주요 변경 사항을 기록합니다.

## [0.1.0] - 2026-07-29

### Added

- JeomseonScriptPack에서 Pool 및 Scope 모듈을 최초 분리했습니다.
- 문자열 빌더, Unity 오브젝트 및 키 기반 오브젝트 풀을 추가했습니다.
- 배열, 리스트, 딕셔너리 및 문자열 빌더용 풀 스코프를 추가했습니다.
- EditMode 단위 테스트를 추가했습니다.

### Changed

- Core 확장 메서드 의존성을 표준 C# 반복문으로 대체했습니다.
