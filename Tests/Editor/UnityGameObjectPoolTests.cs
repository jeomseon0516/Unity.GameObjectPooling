using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using Jeomseon.Unity.GameObjectPooling.Definitions;
using Jeomseon.Unity.GameObjectPooling.Providers;
using Jeomseon.Unity.GameObjectPooling.Factories;
using Jeomseon.Unity.GameObjectPooling.Lifecycle;
using Jeomseon.Unity.GameObjectPooling.Lifetimes;
using Jeomseon.Unity.GameObjectPooling.Pools;
using Jeomseon.Unity.GameObjectPooling.Reset;
using Jeomseon.Unity.GameObjectPooling.Services;
using Jeomseon.Unity.GameObjectPooling.Handles;
using Jeomseon.Unity.GameObjectPooling.Scopes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Jeomseon.Tests
{
    public sealed class UnityGameObjectPoolTests
    {
        [Test]
        public void Dispose_PreservePolicyLeavesActiveInstanceAlive()
        {
            GameObject prefab = CreatePrefab();
            var configuration = new UnityGameObjectPoolConfiguration(
                prefab,
                activeInstanceShutdownPolicy: ActiveInstanceShutdownPolicy.Preserve);
            var pool = new UnityGameObjectPool(configuration);
            GameObject instance = pool.Get();

            pool.Dispose();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.activeSelf, Is.True);
            Assert.That(instance.transform.parent, Is.Null);

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Dispose_DestroyPolicyDestroysActiveInstance()
        {
            GameObject prefab = CreatePrefab();
            var pool = new UnityGameObjectPool(
                new UnityGameObjectPoolConfiguration(prefab));
            GameObject instance = pool.Get();

            pool.Dispose();

            Assert.That(instance == null, Is.True);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Dispose_PreservePolicyDestroysInactiveInstance()
        {
            GameObject prefab = CreatePrefab();
            var configuration = new UnityGameObjectPoolConfiguration(
                prefab,
                activeInstanceShutdownPolicy: ActiveInstanceShutdownPolicy.Preserve);
            var pool = new UnityGameObjectPool(configuration);
            GameObject instance = pool.Get();
            pool.Release(instance);

            pool.Dispose();

            Assert.That(instance == null, Is.True);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Dispose_ExternallyDestroyedActiveInstanceDoesNotThrow()
        {
            GameObject prefab = CreatePrefab();
            var configuration = new UnityGameObjectPoolConfiguration(
                prefab,
                activeInstanceShutdownPolicy: ActiveInstanceShutdownPolicy.Preserve);
            var pool = new UnityGameObjectPool(configuration);
            GameObject instance = pool.Get();
            Object.DestroyImmediate(instance);

            Assert.DoesNotThrow(pool.Dispose);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ScopeShutdown_PreservePolicyInvalidatesHandleAndDetachesActiveInstance()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            GameObjectPoolHandle handle = scope.Register(
                new UnityGameObjectPoolConfiguration(
                    prefab,
                    activeInstanceShutdownPolicy: ActiveInstanceShutdownPolicy.Preserve),
                PoolLifetimeConfiguration.Scope);
            GameObject instance = handle.Spawn();

            scope.Shutdown();

            Assert.That(handle.IsValid, Is.False);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.transform.parent, Is.Null);

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        private sealed class TestComponent : MonoBehaviour, IPoolable
        {
            [ResetOnPoolRelease(7)] public int Value;
            public int GetCount { get; private set; }
            public int ReleaseCount { get; private set; }
            public Vector3 PositionObservedOnGet { get; private set; }

            public void OnGetFromPool()
            {
                GetCount++;
                PositionObservedOnGet = transform.position;
            }

            public void OnReleaseToPool()
            {
                ReleaseCount++;
            }
        }

        private sealed class TestProvider : ComponentPoolProvider<TestComponent>
        {
            public int SpawnHookCount { get; private set; }
            public int DespawnHookCount { get; private set; }

            public TestProvider(GameObjectPoolHandle handle) : base(handle) { }

            protected override void OnSpawned(TestComponent instance)
            {
                SpawnHookCount++;
            }

            protected override void OnDespawned(TestComponent instance)
            {
                DespawnHookCount++;
            }
        }

        private sealed class TrackingFactory : IGameObjectPoolFactory
        {
            private readonly UnityGameObjectPoolFactory _defaultFactory = new();
            public int CreateCount { get; private set; }

            public bool CanCreate(IGameObjectPoolConfiguration configuration)
            {
                return configuration is UnityGameObjectPoolConfiguration;
            }

            public IGameObjectPool Create(
                IGameObjectPoolConfiguration configuration,
                Transform runtimeRoot = null)
            {
                CreateCount++;
                return _defaultFactory.Create(configuration, runtimeRoot);
            }
        }

        private sealed class CountingDefinition : GameObjectPoolDefinition
        {
            public IGameObjectPoolConfiguration Configuration { get; set; }
            public int LifetimeConfigurationCreateCount { get; private set; }

            public override IGameObjectPoolConfiguration CreateConfiguration()
            {
                return Configuration;
            }

            public override IPoolLifetimeConfiguration CreateLifetimeConfiguration()
            {
                LifetimeConfigurationCreateCount++;
                return PoolLifetimeConfiguration.Scope;
            }
        }

        private sealed class ManualLifetimeConfiguration : IPoolLifetimeConfiguration
        {
        }

        private sealed class UnsupportedLifetimeConfiguration : IPoolLifetimeConfiguration
        {
        }

        private sealed class ManualLifetimeHandler : IPoolLifetimeHandler
        {
            private GameObjectPoolHandle _handle;
            private PoolLifetimeRegistrationContext _context;

            public bool IsDisposed { get; private set; }
            public bool ThrowOnValidate { get; set; }
            public bool ThrowOnRegister { get; set; }
            public bool ThrowOnUnregister { get; set; }
            public bool ThrowOnDispose { get; set; }

            public bool CanHandle(IPoolLifetimeConfiguration configuration)
            {
                return configuration is ManualLifetimeConfiguration;
            }

            public void Validate(IPoolLifetimeConfiguration configuration)
            {
                if (ThrowOnValidate)
                {
                    throw new System.InvalidOperationException("Validate failure");
                }

                if (!CanHandle(configuration))
                {
                    throw new System.ArgumentException(nameof(configuration));
                }
            }

            public void Register(
                GameObjectPoolHandle handle,
                IPoolLifetimeConfiguration configuration,
                in PoolLifetimeRegistrationContext context)
            {
                Validate(configuration);
                if (ThrowOnRegister)
                {
                    throw new System.InvalidOperationException("Register failure");
                }

                _handle = handle;
                _context = context;
            }

            public void Unregister(GameObjectPoolHandle handle)
            {
                if (ThrowOnUnregister)
                {
                    throw new System.InvalidOperationException("Unregister failure");
                }

                if (!ReferenceEquals(_handle, handle)) return;
                _handle = null;
            }

            public void Dispose()
            {
                if (ThrowOnDispose)
                {
                    throw new System.InvalidOperationException("Dispose failure");
                }

                IsDisposed = true;
                _handle = null;
            }

            public void Expire()
            {
                _context.Release(_handle);
            }
        }

        [Test]
        public void GetAndRelease_UseUnityStorageAndLifecycleContract()
        {
            GameObject prefab = CreatePrefab();
            UnityGameObjectPoolDefinition definition = CreateDefinition(prefab, prewarmCount: 2);

            using (var pool = CreatePool(definition))
            {
                Assert.That(pool.CountInactive, Is.EqualTo(2));

                Vector3 position = new(2f, 3f, 4f);
                TestComponent component = pool.Get<TestComponent>(
                    PoolSpawnOptions.At(position, Quaternion.identity));
                component.Value = 100;

                Assert.That(component.gameObject.activeSelf, Is.True);
                Assert.That(component.PositionObservedOnGet, Is.EqualTo(position));
                Assert.That(component.GetCount, Is.EqualTo(1));

                pool.Release(component);

                Assert.That(component.gameObject.activeSelf, Is.False);
                Assert.That(component.ReleaseCount, Is.EqualTo(1));
                Assert.That(component.Value, Is.EqualTo(7));
                Assert.That(pool.CountInactive, Is.EqualTo(2));
            }

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Service_CreatesAndReleasesPoolWithoutDefinition()
        {
            GameObject prefab = CreatePrefab();
            var configuration = new UnityGameObjectPoolConfiguration(
                prefab,
                "Runtime pool",
                defaultCapacity: 2,
                maxInactiveCount: 4);

            using (var service = new GameObjectPoolService())
            {
                IGameObjectPool pool = service.CreatePool(configuration);
                GameObject instance = pool.Get();
                pool.Release(instance);

                Assert.That(service.ReleasePool(pool), Is.True);
                Assert.That(service.ReleasePool(pool), Is.False);
                Assert.Throws<System.ObjectDisposedException>(() => pool.Get());
            }

            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Definition_ProducesSeparatePoolAndLifetimeConfigurations()
        {
            GameObject prefab = CreatePrefab();
            UnityGameObjectPoolDefinition definition = CreateDefinition(prefab);

            Assert.That(
                definition.CreateConfiguration(),
                Is.InstanceOf<UnityGameObjectPoolConfiguration>());
            Assert.That(
                definition.CreateLifetimeConfiguration(),
                Is.SameAs(PoolLifetimeConfiguration.Scope));

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ScopeAndProvider_UseSameHandleForRepeatedDefinitionRegistration()
        {
            GameObject prefab = CreatePrefab();
            UnityGameObjectPoolDefinition definition = CreateDefinition(prefab);

            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            GameObjectPoolHandle first = scope.Register(definition);
            GameObjectPoolHandle second = scope.Register(definition);
            var provider = new TestProvider(first);

            TestComponent component = provider.Spawn();
            provider.Despawn(component);

            Assert.That(second, Is.SameAs(first));
            Assert.That(provider.SpawnHookCount, Is.EqualTo(1));
            Assert.That(provider.DespawnHookCount, Is.EqualTo(1));

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void RuntimeRegistrations_CreateIndependentHandles()
        {
            GameObject prefab = CreatePrefab();
            var configuration = new UnityGameObjectPoolConfiguration(prefab);
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();

            GameObjectPoolHandle first = scope.Register(
                configuration,
                PoolLifetimeConfiguration.Scope);
            GameObjectPoolHandle second = scope.Register(
                configuration,
                PoolLifetimeConfiguration.Scope);

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(first.IsValid, Is.True);
            Assert.That(second.IsValid, Is.True);

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Catalog_EvaluatesDefinitionLifetimeOnlyOncePerRegistration()
        {
            GameObject prefab = CreatePrefab();
            CountingDefinition definition =
                ScriptableObject.CreateInstance<CountingDefinition>();
            definition.Configuration = new UnityGameObjectPoolConfiguration(prefab);
            GameObjectPoolCatalog catalog = CreateCatalog(definition);
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();

            scope.LoadCatalog(catalog);

            Assert.That(definition.LifetimeConfigurationCreateCount, Is.EqualTo(1));
            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void CustomLifetimeHandler_CanReleaseRegisteredHandle()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var handler = new ManualLifetimeHandler();
            scope.RegisterLifetimeHandler(handler);

            GameObjectPoolHandle handle = scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new ManualLifetimeConfiguration());
            handler.Expire();

            Assert.That(handle.IsValid, Is.False);
            scope.Shutdown();
            Assert.That(handler.IsDisposed, Is.True);
            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Handles_DoesNotExposeMutableBackingCollection()
        {
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();

            Assert.That(
                scope.Handles,
                Is.Not.InstanceOf<System.Collections.Generic.ICollection<GameObjectPoolHandle>>());

            Object.DestroyImmediate(scopeObject);
        }

        [Test]
        public void UnsupportedLifetimeConfiguration_DoesNotCreatePool()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();

            Assert.Throws<System.NotSupportedException>(() => scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new UnsupportedLifetimeConfiguration()));
            Assert.That(scope.Handles, Is.Empty);

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void LifetimeValidationFailure_DoesNotCreatePool()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var handler = new ManualLifetimeHandler { ThrowOnValidate = true };
            scope.RegisterLifetimeHandler(handler);

            Assert.Throws<System.InvalidOperationException>(() => scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new ManualLifetimeConfiguration()));
            Assert.That(scope.Handles, Is.Empty);

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void LifetimeRegisterFailure_RollsBackPoolAndHandle()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var handler = new ManualLifetimeHandler { ThrowOnRegister = true };
            scope.RegisterLifetimeHandler(handler);

            Assert.Throws<System.InvalidOperationException>(() => scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new ManualLifetimeConfiguration()));
            Assert.That(scope.Handles, Is.Empty);

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void DuplicateLifetimeHandler_IsRejected()
        {
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var handler = new ManualLifetimeHandler();
            scope.RegisterLifetimeHandler(handler);

            Assert.Throws<System.ArgumentException>(() =>
                scope.RegisterLifetimeHandler(handler));

            Object.DestroyImmediate(scopeObject);
        }

        [Test]
        public void LifetimeUnregisterFailure_DoesNotPreventPoolRelease()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var handler = new ManualLifetimeHandler();
            scope.RegisterLifetimeHandler(handler);
            GameObjectPoolHandle handle = scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new ManualLifetimeConfiguration());
            handler.ThrowOnUnregister = true;
            LogAssert.Expect(
                LogType.Exception,
                new System.Text.RegularExpressions.Regex("Unregister failure"));

            Assert.That(scope.Release(handle), Is.True);
            Assert.That(handle.IsValid, Is.False);

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void LifetimeDisposeFailure_DoesNotPreventScopeCleanup()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var handler = new ManualLifetimeHandler { ThrowOnDispose = true };
            scope.RegisterLifetimeHandler(handler);
            GameObjectPoolHandle handle = scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new ManualLifetimeConfiguration());
            LogAssert.Expect(
                LogType.Exception,
                new System.Text.RegularExpressions.Regex("Dispose failure"));

            scope.Shutdown();
            Assert.That(handle.IsValid, Is.False);

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ApplicationLifetime_RequiresDontDestroyOnLoad()
        {
            GameObject prefab = CreatePrefab();
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var configuration = new UnityGameObjectPoolConfiguration(prefab);

            System.InvalidOperationException exception =
                Assert.Throws<System.InvalidOperationException>(() =>
                scope.Register(configuration, PoolLifetimeConfiguration.Application));

            StringAssert.Contains("Dont Destroy On Load", exception.Message);
            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ApplicationLifetime_RequiresRootScopeBeforePoolCreation()
        {
            GameObject prefab = CreatePrefab();
            var parent = new GameObject("Parent");
            var scopeObject = new GameObject("Pool scope");
            scopeObject.transform.SetParent(parent.transform);
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var serializedScope = new SerializedObject(scope);
            serializedScope.FindProperty("dontDestroyOnLoad").boolValue = true;
            serializedScope.ApplyModifiedPropertiesWithoutUndo();
            var configuration = new UnityGameObjectPoolConfiguration(prefab);

            System.InvalidOperationException exception =
                Assert.Throws<System.InvalidOperationException>(() =>
                scope.Register(configuration, PoolLifetimeConfiguration.Application));

            StringAssert.Contains("root GameObject", exception.Message);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void RegisteredCustomFactory_TakesPrecedenceOverDefaultFactory()
        {
            GameObject prefab = CreatePrefab();
            UnityGameObjectPoolDefinition definition = CreateDefinition(prefab);
            var customFactory = new TrackingFactory();

            using (var service = new GameObjectPoolService())
            {
                service.RegisterFactory(customFactory);

                IGameObjectPool pool = service.CreatePool(definition.CreateConfiguration());

                Assert.That(pool, Is.InstanceOf<UnityGameObjectPool>());
                Assert.That(customFactory.CreateCount, Is.EqualTo(1));
            }

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Scope_ExplicitInitializeLoadsCatalogAndInvalidatesHandlesOnShutdown()
        {
            GameObject prefab = CreatePrefab();
            UnityGameObjectPoolDefinition definition =
                CreateDefinition(prefab, prewarmCount: 2);
            GameObjectPoolCatalog catalog = CreateCatalog(definition);
            var scopeObject = new GameObject("Pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();

            var serializedScope = new SerializedObject(scope);
            serializedScope.FindProperty("catalog").objectReferenceValue = catalog;
            serializedScope.FindProperty("defaultDefinition").objectReferenceValue = definition;
            serializedScope.ApplyModifiedPropertiesWithoutUndo();
            scope.Initialize();

            GameObjectPoolHandle handle = scope.DefaultHandle;
            TestComponent component = handle.Spawn<TestComponent>();
            handle.Despawn(component);

            Assert.That(scope.IsInitialized, Is.True);
            Assert.That(handle.TryGetStatistics(out PoolStatistics statistics), Is.True);
            Assert.That(statistics.CountInactive, Is.EqualTo(2));

            scope.Shutdown();
            Assert.That(handle.IsValid, Is.False);
            Assert.Throws<System.ObjectDisposedException>(() => handle.Spawn());

            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ReleaseToDifferentPool_Throws()
        {
            GameObject prefab = CreatePrefab();
            UnityGameObjectPoolDefinition firstDefinition = CreateDefinition(prefab);
            UnityGameObjectPoolDefinition secondDefinition = CreateDefinition(prefab);

            using (var first = CreatePool(firstDefinition))
            using (var second = CreatePool(secondDefinition))
            {
                GameObject instance = first.Get();

                Assert.Throws<System.InvalidOperationException>(() => second.Release(instance));
                first.Release(instance);
            }

            Object.DestroyImmediate(firstDefinition);
            Object.DestroyImmediate(secondDefinition);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void CapabilityContracts_ReportCapacityDiscardAndSupportClear()
        {
            GameObject prefab = CreatePrefab();
            UnityGameObjectPoolDefinition definition = CreateDefinition(prefab);
            using var pool = CreatePool(definition);
            var instances = new GameObject[5];
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i] = pool.Get();
            }

            foreach (GameObject instance in instances)
            {
                pool.Release(instance);
            }

            PoolStatistics statistics = ((IPoolDiagnostics)pool).Statistics;
            Assert.That(statistics.CountInactive, Is.EqualTo(4));
            Assert.That(statistics.CapacityDiscardedCount, Is.EqualTo(1));

            ((IClearablePool)pool).Clear();
            Assert.That(((IPoolDiagnostics)pool).Statistics.CountInactive, Is.Zero);

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(prefab);
        }

        private static GameObject CreatePrefab()
        {
            var prefab = new GameObject("Pool test prefab");
            prefab.AddComponent<TestComponent>();
            return prefab;
        }

        private static UnityGameObjectPoolDefinition CreateDefinition(
            GameObject prefab,
            int prewarmCount = 0)
        {
            UnityGameObjectPoolDefinition definition =
                ScriptableObject.CreateInstance<UnityGameObjectPoolDefinition>();
            definition.name = "Test definition";

            var serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("prefab").objectReferenceValue = prefab;
            serializedDefinition.FindProperty("prewarmCount").intValue = prewarmCount;
            serializedDefinition.FindProperty("defaultCapacity").intValue = 2;
            serializedDefinition.FindProperty("maxInactiveCount").intValue = 4;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static UnityGameObjectPool CreatePool(
            UnityGameObjectPoolDefinition definition)
        {
            return new UnityGameObjectPool(
                (UnityGameObjectPoolConfiguration)definition.CreateConfiguration());
        }

        private static GameObjectPoolCatalog CreateCatalog(
            params GameObjectPoolDefinition[] definitions)
        {
            GameObjectPoolCatalog catalog =
                ScriptableObject.CreateInstance<GameObjectPoolCatalog>();
            var serializedCatalog = new SerializedObject(catalog);
            SerializedProperty items = serializedCatalog.FindProperty("definitions");
            items.arraySize = definitions.Length;
            for (int i = 0; i < definitions.Length; i++)
            {
                items.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }
    }
}
