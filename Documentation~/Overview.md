# Jeomseon Unity GameObject Pooling

[English](Overview.en.md) · [설치](../README.md)

## 목적

Prefab 재사용의 설정·접근·반환·풀 종료를 분리합니다. Unity ObjectPool을 저장소로 사용하고 게임 객체 수명 정책을 추가합니다.

## 사용 흐름

Definition 또는 runtime Configuration을 Scope에 등록 → Handle로 Spawn → 같은 Handle에 Despawn → Scope/정책 종료로 풀 정리. 도메인 Provider는 선택 사항입니다.

## 계약과 제한

최대 비활성 수는 전체 생성 수 제한이 아닙니다. Preserve로 분리한 객체는 직접 파괴해야 합니다. 사용자 수명 훅은 루트 컴포넌트에 둡니다.

## 패키지 요구 사항

- Package ID: `com.jeomseon.unity.game-object-pooling`
- Version: `0.4.0`
- Unity: `6000.6` / `0f1`

## 샘플과 검증

Package Manager > Samples에서 다음 예제를 Import하세요. 포함된 Scene 또는 Editor 도구를 열고 샘플 README의 Setup 절차를 먼저 수행합니다.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [GameObjectPoolingSample](../Samples~/BasicUsage/GameObjectPooling/GameObjectPoolingSample.unity)

[테스트 코드](../Tests)

소비 프로젝트 manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 어셈블리를 실행합니다. 위 링크는 테스트 위치이며 이번 문서 수정으로 새 테스트 실행을 주장하지 않습니다.

## 코드 지도

- [ActiveInstanceShutdownPolicy](../Runtime/Configurations/ActiveInstanceShutdownPolicy.cs)
- [DestroyedInstancePolicy](../Runtime/Configurations/DestroyedInstancePolicy.cs)
- [IGameObjectPoolConfiguration](../Runtime/Configurations/IGameObjectPoolConfiguration.cs)
- [IPoolLifetimeConfiguration](../Runtime/Configurations/IPoolLifetimeConfiguration.cs)
- [OwnerPoolLifetimeConfiguration](../Runtime/Configurations/OwnerPoolLifetimeConfiguration.cs)
- [PoolLifetime](../Runtime/Configurations/PoolLifetime.cs)
- [PoolLifetimeConfiguration](../Runtime/Configurations/PoolLifetimeConfiguration.cs)
- [UnityGameObjectPoolConfiguration](../Runtime/Configurations/UnityGameObjectPoolConfiguration.cs)
- [IAsyncGameObjectPoolFactory](../Runtime/Contracts/IAsyncGameObjectPoolFactory.cs)
- [IClearablePool](../Runtime/Contracts/IClearablePool.cs)
- [IGameObjectPool](../Runtime/Contracts/IGameObjectPool.cs)
- [IGameObjectPoolFactory](../Runtime/Contracts/IGameObjectPoolFactory.cs)
- [IGameObjectPoolService](../Runtime/Contracts/IGameObjectPoolService.cs)
- [IPoolDiagnostics](../Runtime/Contracts/IPoolDiagnostics.cs)
- [IPrewarmablePool](../Runtime/Contracts/IPrewarmablePool.cs)
- [PoolSpawnOptions](../Runtime/Contracts/PoolSpawnOptions.cs)
- [PoolStatistics](../Runtime/Contracts/PoolStatistics.cs)
- [GameObjectPoolCatalog](../Runtime/Definitions/GameObjectPoolCatalog.cs)
- [GameObjectPoolDefinition](../Runtime/Definitions/GameObjectPoolDefinition.cs)
- [UnityGameObjectPoolDefinition](../Runtime/Definitions/UnityGameObjectPoolDefinition.cs)
- [UnityGameObjectPoolFactory](../Runtime/Factories/UnityGameObjectPoolFactory.cs)
- [GameObjectPoolHandle](../Runtime/Handles/GameObjectPoolHandle.cs)
- [IPoolable](../Runtime/Lifecycle/IPoolable.cs)
- [IPoolGetHandler](../Runtime/Lifecycle/IPoolGetHandler.cs)
- [IPoolReleaseHandler](../Runtime/Lifecycle/IPoolReleaseHandler.cs)
- [IPoolLifetimeHandler](../Runtime/Lifetimes/IPoolLifetimeHandler.cs)
- [PoolLifetimeRegistrationContext](../Runtime/Lifetimes/PoolLifetimeRegistrationContext.cs)
- [UnityGameObjectPool](../Runtime/Pools/UnityGameObjectPool.cs)
- [ComponentPoolProvider](../Runtime/Providers/ComponentPoolProvider.cs)
- [GameObjectPoolRegistration](../Runtime/Registrations/GameObjectPoolRegistration.cs)
- [PoolRegistrationResult](../Runtime/Registrations/PoolRegistrationResult.cs)
- [PoolRegistrationStatus](../Runtime/Registrations/PoolRegistrationStatus.cs)
- [ResetOnPoolReleaseAttribute](../Runtime/Reset/ResetOnPoolReleaseAttribute.cs)
- [GameObjectPoolScope](../Runtime/Scopes/GameObjectPoolScope.cs)
- [GameObjectPoolScopeAsyncExtensions](../Runtime/Scopes/GameObjectPoolScopeAsyncExtensions.cs)
- [GameObjectPoolService](../Runtime/Services/GameObjectPoolService.cs)

[상세 설계](Design.md)
