# Jeomseon Unity GameObject Pooling

Provides configurable and replaceable pooling for Unity GameObjects and Components.

## Requirements

- Unity 6000.6.0f1 or newer

## Install via OpenUPM

Register the OpenUPM scoped registry once in your project's `Packages/manifest.json`.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.game-object-pooling": "0.4.0"
  }
}
```

## Install via Git URL

Enter the following URL in Unity Package Manager's `Install package from git URL`.

```text
https://github.com/jeomseon0516/Unity.GameObjectPooling.git#v0.4.0
```

## Features

- Replaceable `IGameObjectPool`, `IGameObjectPoolFactory`, and runtime configuration contracts
- A default implementation backed by Unity `ObjectPool<GameObject>` and ScriptableObject definitions
- An inheritable `ComponentPoolProvider<T>` that does not create another storage
- Unity `GameObject` and `Component` object pools
- `ResetOnPoolReleaseAttribute` for restoring fields and properties on return

Regular call sites depend on a `GameObjectPoolHandle` returned by `GameObjectPoolScope`, or on a project-facing Provider. Definitions and runtime configurations both become registrations, separating creation input from runtime identity.

## GameObject pool setup

1. Create a definition from `Tool/GameObject Pooling/Unity GameObject Pool` and configure its prefab and policies.
2. Add `GameObjectPoolScope` to a scene GameObject and assign its default Definition.
3. Assign a Catalog only when multiple pools must be registered and preloaded together. A Catalog is not used as a runtime pool lookup key.

Each definition produces a shared registration from separate pool-construction and lifetime configurations. The scope processes the built-in `Scope`, `Scene`, and `Application` policies. Application lifetime requires a root scope with `Dont Destroy On Load` enabled.

Use the runtime-only `OwnerPoolLifetimeConfiguration` when a scene object owns a pool. A
GameObject or Component can be the owner, and the pool is released only after actual
destruction, not when the owner is disabled. The same owner can manage multiple pools. Scene
object references are intentionally not stored in ScriptableObject Definitions.

```csharp
GameObjectPoolHandle ownedHandle = scope.Register(
    poolConfiguration,
    new OwnerPoolLifetimeConfiguration(this));
```

A registration can also be created in code without a ScriptableObject. Runtime registrations return independent handles, while a Definition resolves a shared handle within one scope.

```csharp
var poolConfiguration =
    new UnityGameObjectPoolConfiguration(prefab, "Runtime Enemies");
GameObjectPoolHandle handle = scope.Register(
    poolConfiguration,
    PoolLifetimeConfiguration.Scene);

Enemy enemy = handle.Spawn<Enemy>(options);
handle.Despawn(enemy);
```

Project-specific Mesh, Material, and Texture variants remain a `ComponentPoolProvider<T>` extension concern. Prefer separate prefab variants and definitions for fixed variants.

Choose the access layer by purpose. `Scope.Spawn/Despawn` is a convenience API for one
default pool, `GameObjectPoolHandle` is the normal explicit pool access, and
`ComponentPoolProvider<T>` is an optional project or domain adapter. In Play Mode, the Scope
Inspector reports every registered Handle, including runtime registrations outside a Catalog.

For finished projects, prefer a domain-specific Provider interface and implementation in
gameplay code. A Provider receives only a scope-owned `GameObjectPoolHandle` and does not own
the pool directly. `GameObjectPoolService`, factories, and `IGameObjectPool` remain public
low-level APIs for custom storage and composition roots.

### Synchronous and asynchronous creation

Consumers select the synchronous or `Awaitable`-based API according to the resource source.
Once registration completes, both paths use the same synchronous `Spawn` and `Despawn`
operations.

```csharp
GameObjectPoolHandle immediate = scope.Register(definition);
GameObjectPoolHandle loaded = await scope.RegisterAsync(
    definition,
    destroyCancellationToken);
```

Coroutine- or event-oriented code can use the callback convenience API over the same async
path. The callback runs exactly once on the main thread and receives success, cancellation,
or failure through one `PoolRegistrationResult`.

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

`RegisterAsync` prefers a matching `IAsyncGameObjectPoolFactory` and falls back to a
synchronous `IGameObjectPoolFactory`. `Register` never blocks on an asynchronous factory
through `.Wait()`, `.Result`, or `GetAwaiter().GetResult()`.

Addressables and network integration packages implement `IAsyncGameObjectPoolFactory`.
Ownership of external load handles and resources moves to the returned `IGameObjectPool`,
whose `Dispose` releases them. The core GameObject Pooling package does not directly reference a
specific loader type.

For a project-specific lifetime, store policy data in `IPoolLifetimeConfiguration`, implement
its execution in `IPoolLifetimeHandler`, and register the handler with a Scope. The Scope takes
ownership and disposes the handler on shutdown; the latest compatible handler takes precedence.
An existing shared Handle keeps its initially selected handler. Custom handlers must be registered
again after shutdown and reinitialization, and `Validate` must remain free of side effects.

```csharp
scope.RegisterLifetimeHandler(new RoundLifetimeHandler(roundService));

GameObjectPoolHandle handle = scope.Register(
    poolConfiguration,
    new RoundLifetimeConfiguration(roundId));
```

| API level | Primary types | Purpose |
| --- | --- | --- |
| Convenience | `Scope.Spawn/Despawn` | One default pool and prototypes |
| Standard | Scope, Handle, Definition | Explicit registration and use |
| Project boundary | Domain Provider | Recommended dependency for finished gameplay |
| Extension | Registration, Configuration, Lifetime Handler | Creation and lifetime policies |
| Low-level | Service, Factory, `IGameObjectPool` | Custom storage and composition roots |

The `Basic Usage` sample includes Definition-based access, runtime-configuration access, project-facing Provider access, and a manual Application-lifetime verification environment.

## License

[MIT License](./LICENSE.md)
