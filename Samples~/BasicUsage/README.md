# GameObject Pooling 기본 예제

`GameObjectPooling/GameObjectPoolingSample` 씬은 일반 호출부터 저장소 확장까지 단계별로 구성되어 있습니다. 숫자가 높아질수록 더 낮은 수준의 API를 직접 다룹니다.

## 가장 간단한 호출부

Definition 방식과 런타임 Configuration 방식 모두 실제 사용 코드는 같습니다.

```csharp
GameObjectPoolHandle handle = scope.DefaultHandle;
PooledSampleActor actor = handle.Spawn<PooledSampleActor>(options);
handle.Despawn(actor);
```

호출부는 Definition 또는 런타임 Configuration이 만든 `GameObjectPoolHandle`만 사용합니다. 생성 입력은 Registration에, 소유권과 수명은 Scope에 분리됩니다.

## 01 — Definition Access

`GameObject Pool Scope`가 `SampleGameObjectPool` Definition을 직접 참조합니다. `GameObjectPoolingSample`에는 Scope만 연결되어 있으며 Definition 필드가 없습니다.

- 권장 대상: Inspector 중심 설정, 고정된 Prefab Variant
- 호출 API: `scope.Spawn` / `scope.Despawn` 또는 `scope.DefaultHandle`
- 설정 위치: `UnityGameObjectPoolDefinition`

## 02 — Runtime Configuration Access

`RuntimeGameObjectPoolScopeConfigurator`가 실행 순서상 먼저 런타임 Registration을 Scope에 등록합니다.

```csharp
var configuration = new UnityGameObjectPoolConfiguration(prefab);
GameObjectPoolHandle handle = scope.Register(
    configuration,
    PoolLifetimeConfiguration.Scene);
scope.SetDefault(handle);
```

구성 이후 `RuntimeConfiguredPoolingSample`은 생성 Configuration 대신 `scope.DefaultHandle`만 사용합니다.

- 구분: `MID-LEVEL CUSTOMIZATION / 중간 수준 사용자 정의`
- 권장 대상: 런타임에 결정되는 Prefab, 동적 콘텐츠
- 사용자 책임: 생성 Configuration과 수명 Configuration 선택

## 03·04 — Domain Provider Access

- `03 Provider With Definition`: Definition으로 등록된 Handle 사용
- `04 Provider With Runtime Configuration`: 런타임에 등록된 Handle 사용

두 호출부는 동일합니다.

```csharp
ISampleActorProvider provider = new SampleActorPoolProvider(scope.DefaultHandle);
PooledSampleActor actor = provider.Spawn("Provider Actor", position, rotation, parent);
provider.Despawn(actor);
```

- 구분: `HIGH-LEVEL CUSTOMIZATION / 고수준 사용자 정의`
- 권장 대상: 실제 게임 로직
- 의존성: `ISampleActorProvider` 같은 프로젝트 도메인 계약
- 장점: Pool, Definition, Addressables 또는 Instantiate 구현을 호출부에서 숨김

`ComponentPoolProvider<T>`는 타입 안전한 공통 `Spawn/Despawn` 흐름과 확장 훅을 제공합니다. `SampleActorPoolProvider`는 이를 상속하여 도메인 초기화만 추가합니다.

## 05 — Low-Level Custom Factory

`LowLevelCustomPoolSample`은 다음 저수준 확장 지점을 한 번에 보여줍니다.

1. `CountingPoolConfiguration`: 사용자 Configuration
2. `CountingGameObjectPoolFactory`: 사용자 Factory 선택
3. `CountingGameObjectPool`: `IGameObjectPool` Decorator
4. `scope.RegisterFactory`: 풀이 등록되기 전 Factory 등록

- 구분: `LOW-LEVEL CUSTOMIZATION / 저수준 사용자 정의`
- 권장 대상: 저장소 교체, 진단 계층, 특수 할당 정책
- 주의: 일반 게임 로직에서는 이 API에 직접 의존하지 않습니다.

## 06 — Application Lifetime Verification

`06 Application Lifetime Verification`은 루트 GameObject에 영속 Scope와 Application
수명 Definition을 올바르게 구성한 Play Mode 검증 환경입니다.

1. `GameObjectPooling/GameObjectPoolingSample` 씬을 엽니다.
2. Play Mode에 진입합니다.
3. `06 Application Lifetime Verification`을 선택합니다.
4. `ApplicationLifetimePoolingSample`의 Context Menu에서
   `Run Application Lifetime Check`를 실행합니다.
5. Console에서 `[PASS]` 로그를 확인합니다.

검사는 임시 Scene을 만든 뒤 원래 샘플 Scene을 언로드하고 다음 항목을 확인합니다.

- 루트 `GameObjectPoolScope`가 `DontDestroyOnLoad` Scene에 유지됨
- 기존 `GameObjectPoolHandle`이 계속 유효함
- Scene 언로드 후에도 같은 풀에서 객체를 생성하고 반환할 수 있음

Play Mode를 종료하면 원래 샘플 Scene으로 복원됩니다. Scope를 다른 GameObject의 자식으로
옮기거나 `Dont Destroy On Load`를 끄면 Inspector와 Console에서 정책 오류를 확인할 수 있습니다.

## 07 — Custom Round Lifetime

`07 Custom Round Lifetime`은 프로젝트별 수명 정책을 구현하는 완전한 예제입니다.

- `RoundLifetimeConfiguration`: 풀을 소유하는 Round ID 데이터
- `SampleRoundLifetimeController`: 프로젝트의 Round 종료 이벤트 소스
- `RoundPoolLifetimeHandler`: Round 종료 시 Context를 통해 Scope에 Handle 해제 요청
- `RoundLifetimePoolingSample`: Handler 등록과 런타임 풀 구성

Play Mode에서 `RoundLifetimePoolingSample`의 Context Menu를 다음 순서로 실행합니다.

1. `Spawn Round Object`
2. `End Round And Release Pool`
3. Console의 `[PASS] Round pool was released` 확인

Handler는 Scope에 등록되는 순간 소유권을 이전합니다. 이미 생성된 Handle은 최초 선택된
Handler를 유지하며, Scope를 `Shutdown`한 뒤 다시 초기화하면 사용자 Handler도 다시 등록해야
합니다. `Validate`는 부작용 없이 풀 생성 가능 여부만 검사하도록 구현합니다.

## 네이밍과 파일 역할

- `GameObjectPoolingSample`: 구성 출처를 모르는 간결한 Scope 호출
- `RuntimeGameObjectPoolScopeConfigurator`: 런타임 Composition Root
- `ISampleActorProvider`: 게임 로직이 의존하는 도메인 계약
- `SampleActorPoolProvider`: Handle을 도메인 Provider로 변환
- `ProviderPoolingSample`: 고수준 Provider 호출부
- `LowLevelCustomPoolSample`: Configuration/Factory/Pool 저수준 확장
- `ApplicationLifetimePoolingSample`: Application 수명 Play Mode 검증
- `RoundPoolLifetimeHandler`: 프로젝트별 Round 수명 실행 예시
- `RoundLifetimePoolingSample`: 사용자 Handler Composition Root
- `PooledSampleActor`: Provider가 초기화하는 샘플 Component
