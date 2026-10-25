# Jeomseon Unity GameObject Pooling

[한국어](Overview.md) · [Installation](../README.en.md)

## Purpose

Separates Prefab reuse configuration, access, return, and shutdown. Unity ObjectPool supplies storage; the package adds GameObject lifetime policies.

## Usage flow

Register a Definition or runtime Configuration in a Scope, Spawn through its Handle, Despawn through the same Handle, then let the Scope/policy shut down the pool. Domain Providers are optional.

## Contracts and limitations

The inactive limit is not a total-spawn limit. Destroy preserved detached objects yourself. Put custom lifecycle handlers on the Prefab root.

## Package requirements

- Package ID: `com.jeomseon.unity.game-object-pooling`
- Version: `0.4.0`
- Unity: `6000.6` / `0f1`

## Samples and verification

Import the following from Package Manager > Samples. Open the included Scene or Editor tool; follow any Setup step in its sample README.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [GameObjectPoolingSample](../Samples~/BasicUsage/GameObjectPooling/GameObjectPoolingSample.unity)

[Test sources](../Tests)

Add the package ID to manifest `testables`, then run the relevant Unity Test Runner assemblies. These links describe available tests, not a new successful test run.

## Source map

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

[Detailed design](Design.en.md)
