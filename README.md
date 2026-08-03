# Jeomseon Unity GameObject Pooling

Unity GameObject와 Component를 위한 설정 가능하고 교체 가능한 풀링 기능을 제공합니다.

## 요구 사항

- Unity 6000.3.15f1 이상

## OpenUPM으로 설치

프로젝트의 `Packages/manifest.json`에 OpenUPM scoped registry를 한 번 등록합니다.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon.unity"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.game-object-pooling": "0.1.2"
  }
}
```

## Git URL로 설치

Unity Package Manager의 `Install package from git URL`에 다음 URL을 입력합니다.

```text
https://github.com/jeomseon0516/Unity.GameObjectPooling.git#v0.1.2
```

## 로컬 개발

개발용 Unity 프로젝트의 `Packages/manifest.json`에 로컬 저장소를 연결합니다.

```json
{
  "dependencies": {
    "com.jeomseon.unity.game-object-pooling": "file:../../Jeomseon.Unity.GameObjectPooling"
  },
  "testables": [
    "com.jeomseon.unity.game-object-pooling"
  ]
}
```

## 포함 기능

- 사용자 구현으로 교체 가능한 `IGameObjectPool`, `IGameObjectPoolFactory` 및 런타임 Configuration 계약
- Unity `ObjectPool<GameObject>` 기반 기본 구현과 ScriptableObject 풀 정의
- 별도 저장소를 만들지 않는 상속 가능 `ComponentPoolProvider<T>`
- Unity `GameObject` 및 `Component` 오브젝트 풀
- 풀 반환 시 필드와 프로퍼티를 복원하는 `ResetOnPoolReleaseAttribute`

일반 호출부는 `GameObjectPoolScope`가 반환한 `GameObjectPoolHandle` 또는 프로젝트 Provider에 의존합니다. Definition과 런타임 Configuration은 모두 Registration으로 변환되므로 생성 방식과 런타임 식별이 분리됩니다.

## GameObject 풀 설정

1. `Tool/GameObject Pooling/Unity GameObject Pool`에서 Definition을 만들고 Prefab과 정책을 설정합니다.
2. 씬의 GameObject에 `GameObjectPoolScope`를 추가하고 기본 Definition을 연결합니다.
3. 여러 풀을 일괄 등록하고 Preload해야 할 때만 선택적으로 Catalog를 연결합니다. Catalog는 런타임 풀 조회 키로 사용되지 않습니다.

Definition은 풀 생성 Configuration과 수명 Configuration으로 공유 Registration을 만듭니다. Scope는 `PoolLifetimeConfiguration`의 `Scope`, `Scene`, `Application` 정책을 처리하며, `Application` 수명은 `Dont Destroy On Load`가 활성화된 루트 Scope에서만 사용할 수 있습니다.

ScriptableObject 없이 코드에서 Registration을 만들 수도 있습니다. 런타임 등록은 독립 Handle을 반환하며 Definition 등록은 같은 Scope에서 공유 Handle을 반환합니다.

```csharp
var poolConfiguration =
    new UnityGameObjectPoolConfiguration(prefab, "Runtime Enemies");
GameObjectPoolHandle handle = scope.Register(
    poolConfiguration,
    PoolLifetimeConfiguration.Scene);

Enemy enemy = handle.Spawn<Enemy>(options);
handle.Despawn(enemy);
```

Mesh, Material, Texture 같은 프로젝트별 Variant 적용은 `ComponentPoolProvider<T>` 사용자 확장 영역입니다. 고정된 Variant는 Prefab Variant와 별도 Definition을 사용하는 것을 권장합니다.

접근 계층은 용도에 따라 선택합니다. `Scope.Spawn/Despawn`은 단일 기본 풀 편의 API,
`GameObjectPoolHandle`은 일반적인 명시적 풀 접근, `ComponentPoolProvider<T>`는 선택적인
프로젝트·도메인 어댑터입니다. Play Mode의 Scope Inspector는 Catalog 여부와 관계없이
Definition 및 런타임 Configuration으로 등록된 모든 Handle의 통계를 표시합니다.

완성된 프로젝트의 게임 로직에서는 도메인별 Provider 인터페이스와 구현을 두는 것을
권장합니다. Provider는 Scope가 소유한 `GameObjectPoolHandle`만 전달받으며 Pool을 직접
소유하지 않습니다. `GameObjectPoolService`, Factory 및 `IGameObjectPool`은 사용자 저장소나
Composition Root를 구현하는 저수준 확장 API입니다.

### 동기 및 비동기 생성

호출부는 리소스 공급 방식에 따라 동기 또는 `Awaitable` 기반 API를 선택합니다. 등록이
완료된 뒤의 `Spawn`과 `Despawn`은 두 경로 모두 동일한 동기 API를 사용합니다.

```csharp
GameObjectPoolHandle immediate = scope.Register(definition);
GameObjectPoolHandle loaded = await scope.RegisterAsync(
    definition,
    destroyCancellationToken);
```

Coroutine나 이벤트 중심 코드에서는 동일한 비동기 경로의 Callback 편의 API를 사용할 수
있습니다. Callback은 메인 스레드에서 정확히 한 번 호출되며 성공·취소·실패가 하나의
`PoolRegistrationResult`로 전달됩니다.

```csharp
scope.RegisterAsync(
    definition,
    result =>
    {
        if (result.IsCanceled) return;
        if (result.IsFailed)
        {
            Debug.LogException(result.Exception);
            return;
        }

        GameObjectPoolHandle handle = result.Handle;
    },
    destroyCancellationToken);
```

`RegisterAsync`는 지원하는 `IAsyncGameObjectPoolFactory`를 우선 선택하고, 없으면 동기
`IGameObjectPoolFactory`로 fallback합니다. 반대로 `Register`는 비동기 Factory를 기다리지
않으며 `.Wait()`, `.Result` 또는 `GetAwaiter().GetResult()`로 블로킹하지 않습니다.

Addressables나 네트워크 연동 패키지는 `IAsyncGameObjectPoolFactory`를 구현합니다. 외부
로드 핸들과 리소스의 소유권은 Factory가 반환하는 `IGameObjectPool` 구현으로 이전하고,
그 Pool의 `Dispose`에서 Addressables `Release`나 네트워크 리소스 해제를 수행해야 합니다.
GameObject Pooling 핵심 패키지는 특정 로더 타입을 직접 참조하지 않습니다.

프로젝트별 수명주기는 `IPoolLifetimeConfiguration`에 정책 데이터를 두고
`IPoolLifetimeHandler`에 실행 로직을 구현한 뒤 Scope에 등록합니다. Scope는 Handler의
소유권을 넘겨받아 종료 시 Dispose하며, 나중에 등록한 호환 Handler를 우선 선택합니다.
이미 생성된 Handle은 최초 선택된 Handler를 계속 사용합니다. `Shutdown` 후 재초기화할
때는 사용자 Handler를 다시 등록해야 하며 `Validate`는 부작용 없이 구현해야 합니다.

```csharp
scope.RegisterLifetimeHandler(new RoundLifetimeHandler(roundService));

GameObjectPoolHandle handle = scope.Register(
    poolConfiguration,
    new RoundLifetimeConfiguration(roundId));
```

| API 수준 | 주요 타입 | 용도 |
| --- | --- | --- |
| 편의 | `Scope.Spawn/Despawn` | 단일 기본 풀과 프로토타입 |
| 일반 | Scope, Handle, Definition | 명시적인 풀 등록과 사용 |
| 프로젝트 경계 | 도메인별 Provider | 완성된 게임 로직의 권장 의존성 |
| 확장 | Registration, Configuration, Lifetime Handler | 생성·수명 정책 확장 |
| 저수준 | Service, Factory, `IGameObjectPool` | 사용자 저장소와 Composition Root |

`Basic Usage` 샘플에는 Definition 접근, 런타임 Configuration 접근, 프로젝트 Provider를 통한 간접 접근, Application 수명 수동 검증 환경이 포함됩니다.

## 테스트

패키지를 `testables`에 등록한 후 Unity Test Runner의 EditMode에서 실행합니다.

## 라이선스

[MIT License](./LICENSE.md)
