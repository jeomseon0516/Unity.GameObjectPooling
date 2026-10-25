# GameObject Pooling: design and behavior

[한국어](Design.md) · [Installation and APIs](../README.en.md)

## Problem and boundaries

Unity `ObjectPool<T>` supplies reusable storage. This package adds Prefab configuration, Component
access, external-destruction policies, reset-on-return behavior, and Scope/Scene/Application/owner
lifetimes. Storage remains backed by Unity's pool API.

| Type | Responsibility | Consumer contract |
| --- | --- | --- |
| `UnityGameObjectPoolDefinition` | Shared Inspector configuration | Does not store runtime pool state |
| `GameObjectPoolScope` | Registration and lifetime entry point | Supplies the default or registered Handle |
| `GameObjectPoolHandle` | Spawn/Despawn for a particular pool | `IsValid` becomes false on shutdown |
| `ComponentPoolProvider<T>` | Project-specific domain access | Does not independently own the pool |
| `IGameObjectPoolFactory` | Creates pools from configuration | Extension point for custom implementations |
| `IPoolLifetimeHandler` | Executes lifetime policies | Ownership transfers to the registering Scope |

Definition registration shares a Handle within the same Scope. Runtime Configuration registration
creates independent Handles. A Catalog optionally batches registration/preload; it is not a global
lookup key for gameplay code. The flow is Configuration → Scope/Registration → Factory → Pool,
with consumers accessing the Pool through a Handle or domain Provider.

## Reuse and shutdown

1. Spawn obtains an inactive instance and applies parent/pose.
2. The instance is tracked as active, enabled, then receives `IPoolGetHandler` callbacks.
3. Return through the same Handle's Despawn.
4. `IPoolReleaseHandler` runs, followed by configured `[ResetOnPoolRelease]` restoration.
5. The instance is disabled, parented to the pool root, and returned to storage.

Lifecycle handlers are discovered on the root GameObject at creation. Child handlers are not
automatically invoked. `Spawn<T>` throws when the Prefab root does not contain T.

Runtime configuration defaults are prewarm 0, capacity 10, maximum inactive 100, collection checks
enabled, attribute reset enabled, `WarnAndReplace` for external destruction, and `Destroy` for
active instances on shutdown. The inactive limit is not a total-spawn limit; excess returns are destroyed.

Application lifetime requires a persistent root Scope. Owner lifetime ends on destruction, not
deactivation. Inactive instances are destroyed on shutdown. `Preserve` detaches active instances
from the pool; their new owner must destroy them and cannot return them through the invalid Handle.

## Failure and async behavior

- Foreign-pool objects, duplicate returns, and null returns are invalid and throw in the default pool.
- Spawn/Despawn on an invalid Handle throws `ObjectDisposedException`.
- External Destroy is handled by `DestroyedInstancePolicy`; use Despawn for normal returns.
- Async work belongs to registration/creation. Spawn and Despawn remain synchronous afterward.
- `RegisterAsync` prefers an async Factory, otherwise uses a synchronous one. Synchronous Register
  never blocks waiting for an async Factory.
- A custom Factory's acquired loader resources must be released by the returned pool's Dispose.

## Run and inspect

Import Basic Usage in Package Manager and follow the [sample instructions](../Samples~/BasicUsage/README.md).
Compare [minimal access](../Samples~/BasicUsage/GameObjectPoolingSample.cs) with the
[Provider example](../Samples~/BasicUsage/ProviderPoolingSample.cs).
Read [Scope](../Runtime/Scopes/GameObjectPoolScope.cs),
[Handle](../Runtime/Handles/GameObjectPoolHandle.cs), then
[Pool](../Runtime/Pools/UnityGameObjectPool.cs).
[Tests](../Tests) cover return, shutdown, and external destruction. Add the package ID to the
consumer manifest's `testables` and run Unity Test Runner.
