using System.Collections;
using System.Threading;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using Jeomseon.Unity.GameObjectPooling.Definitions;
using Jeomseon.Unity.GameObjectPooling.Pools;
using Jeomseon.Unity.GameObjectPooling.Services;
using Jeomseon.Unity.GameObjectPooling.Handles;
using Jeomseon.Unity.GameObjectPooling.Scopes;
using Jeomseon.Unity.GameObjectPooling.Registrations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Jeomseon.Tests
{
    public sealed class UnityGameObjectPoolPlayModeTests
    {
        private sealed class TestDefinition : GameObjectPoolDefinition
        {
            public IGameObjectPoolConfiguration Configuration { get; set; }

            public override IGameObjectPoolConfiguration CreateConfiguration() => Configuration;

            public override IPoolLifetimeConfiguration CreateLifetimeConfiguration() =>
                PoolLifetimeConfiguration.Scope;
        }

        private sealed class AsyncTrackingFactory : IAsyncGameObjectPoolFactory
        {
            private readonly Jeomseon.Unity.GameObjectPooling.Factories.UnityGameObjectPoolFactory
                _factory = new();

            public int CreateCount { get; private set; }

            public bool CanCreate(IGameObjectPoolConfiguration configuration) =>
                configuration is UnityGameObjectPoolConfiguration;

            public async Awaitable<IGameObjectPool> CreateAsync(
                IGameObjectPoolConfiguration configuration,
                Transform runtimeRoot = null,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                CreateCount++;
                return _factory.Create(configuration, runtimeRoot);
            }
        }

        private sealed class EnableObserver : MonoBehaviour
        {
            public Vector3 PositionOnEnable { get; private set; }

            private void OnEnable()
            {
                PositionOnEnable = transform.position;
            }
        }

        [UnityTest]
        public IEnumerator Get_AppliesSpawnPoseBeforeOnEnable()
        {
            var prefab = new GameObject("PlayMode pool prefab");
            prefab.SetActive(false);
            prefab.AddComponent<EnableObserver>();
            TestDefinition definition = CreateDefinition(prefab);
            using var pool = CreatePool(definition);
            Vector3 position = new(4f, 5f, 6f);

            EnableObserver observer = pool.Get<EnableObserver>(
                PoolSpawnOptions.At(position, Quaternion.identity));

            Assert.That(observer.PositionOnEnable, Is.EqualTo(position));
            pool.Release(observer);

            Object.Destroy(definition);
            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scope_InitializeOnAwake_InitializesInPlayMode()
        {
            var scopeObject = new GameObject("Awake pool scope");
            scopeObject.SetActive(false);
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();

            Assert.That(scope.IsInitialized, Is.False);

            scopeObject.SetActive(true);
            yield return null;

            Assert.That(scope.IsInitialized, Is.True);

            Object.Destroy(scopeObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scope_OnDestroy_InvalidatesOwnedHandles()
        {
            var prefab = new GameObject("Destroyed scope prefab");
            prefab.SetActive(false);
            var scopeObject = new GameObject("Destroyed pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            GameObjectPoolHandle handle = scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                PoolLifetimeConfiguration.Scope);

            Assert.That(handle.IsValid, Is.True);

            Object.Destroy(scopeObject);
            yield return null;

            Assert.That(handle.IsValid, Is.False);
            Assert.Throws<System.ObjectDisposedException>(() => handle.Spawn());

            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReplacePolicy_RecoversFromExternallyDestroyedInactiveInstance()
        {
            var prefab = new GameObject("Replace policy prefab");
            prefab.SetActive(false);
            TestDefinition definition = CreateDefinition(
                prefab,
                DestroyedInstancePolicy.Replace);
            using var pool = CreatePool(definition);
            GameObject first = pool.Get();
            pool.Release(first);
            Object.Destroy(first);
            yield return null;

            GameObject replacement = pool.Get();

            Assert.That(replacement, Is.Not.Null);
            Assert.That(replacement, Is.Not.SameAs(first));
            Assert.That(((IPoolDiagnostics)pool).Statistics.DestroyedCount, Is.GreaterThanOrEqualTo(1));
            pool.Release(replacement);

            Object.Destroy(definition);
            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RuntimeConfiguration_SceneLifetimeReleasesPoolWhenSceneUnloads()
        {
            Scene originalScene = SceneManager.GetActiveScene();
            Scene ownerScene = SceneManager.CreateScene("Pool lifetime test scene");
            SceneManager.SetActiveScene(ownerScene);

            var prefab = new GameObject("Scene lifetime prefab");
            prefab.SetActive(false);
            var scopeObject = new GameObject("Persistent pool scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            Object.DontDestroyOnLoad(scopeObject);
            var poolConfiguration = new UnityGameObjectPoolConfiguration(prefab);
            GameObjectPoolHandle handle = scope.Register(
                poolConfiguration,
                PoolLifetimeConfiguration.Scene);

            yield return SceneManager.UnloadSceneAsync(ownerScene);

            Assert.That(handle.IsValid, Is.False);
            Assert.Throws<System.ObjectDisposedException>(() => handle.Spawn());

            if (originalScene.IsValid() && originalScene.isLoaded)
            {
                SceneManager.SetActiveScene(originalScene);
            }

            Object.Destroy(scopeObject);
            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OwnerLifetime_ReleasesPoolOnlyAfterOwnerIsDestroyed()
        {
            var prefab = new GameObject("Owner lifetime prefab");
            prefab.SetActive(false);
            var scopeObject = new GameObject("Owner lifetime scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var owner = new GameObject("Pool owner");
            GameObjectPoolHandle handle = scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new OwnerPoolLifetimeConfiguration(owner));

            owner.SetActive(false);
            yield return null;
            Assert.That(handle.IsValid, Is.True, "Disabling an owner must not release its pool.");

            Object.Destroy(owner);
            yield return null;
            yield return null;
            Assert.That(handle.IsValid, Is.False);
            Assert.Throws<System.ObjectDisposedException>(() => handle.Spawn());

            Object.Destroy(scopeObject);
            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OwnerLifetime_ComponentDestructionDoesNotRequireGameObjectDestruction()
        {
            var prefab = new GameObject("Component owner prefab");
            prefab.SetActive(false);
            var scopeObject = new GameObject("Component owner scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var ownerObject = new GameObject("Component owner GameObject");
            BoxCollider owner = ownerObject.AddComponent<BoxCollider>();
            GameObjectPoolHandle handle = scope.Register(
                new UnityGameObjectPoolConfiguration(prefab),
                new OwnerPoolLifetimeConfiguration(owner));

            Object.Destroy(owner);
            yield return null;
            yield return null;

            Assert.That(ownerObject, Is.Not.Null);
            Assert.That(handle.IsValid, Is.False);

            Object.Destroy(ownerObject);
            Object.Destroy(scopeObject);
            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OwnerLifetime_ReleasesEveryPoolOwnedByOneObject()
        {
            var firstPrefab = new GameObject("First owner prefab");
            var secondPrefab = new GameObject("Second owner prefab");
            firstPrefab.SetActive(false);
            secondPrefab.SetActive(false);
            var scopeObject = new GameObject("Shared owner scope");
            GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
            var owner = new GameObject("Shared pool owner");
            var lifetime = new OwnerPoolLifetimeConfiguration(owner);
            GameObjectPoolHandle first = scope.Register(
                new UnityGameObjectPoolConfiguration(firstPrefab), lifetime);
            GameObjectPoolHandle second = scope.Register(
                new UnityGameObjectPoolConfiguration(secondPrefab), lifetime);

            Object.Destroy(owner);
            yield return null;
            yield return null;

            Assert.That(first.IsValid, Is.False);
            Assert.That(second.IsValid, Is.False);

            Object.Destroy(scopeObject);
            Object.Destroy(firstPrefab);
            Object.Destroy(secondPrefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ServiceAsync_PrefersAsyncFactoryAndFallsBackToSyncFactory()
        {
            async Awaitable TestImplementation()
            {
                var prefab = new GameObject("Async factory prefab");
                prefab.SetActive(false);
                var configuration = new UnityGameObjectPoolConfiguration(prefab);

                using (var fallbackService = new GameObjectPoolService())
                {
                    IGameObjectPool fallbackPool =
                        await fallbackService.CreatePoolAsync(configuration);
                    Assert.That(fallbackPool, Is.Not.Null);
                }

                var asyncFactory = new AsyncTrackingFactory();
                using (var service = new GameObjectPoolService())
                {
                    service.RegisterAsyncFactory(asyncFactory);
                    IGameObjectPool pool = await service.CreatePoolAsync(configuration);

                    Assert.That(pool, Is.Not.Null);
                    Assert.That(asyncFactory.CreateCount, Is.EqualTo(1));
                }

                Object.Destroy(prefab);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator ScopeAsync_CoalescesConcurrentDefinitionRegistration()
        {
            async Awaitable TestImplementation()
            {
                var prefab = new GameObject("Async scope prefab");
                prefab.SetActive(false);
                TestDefinition definition = CreateDefinition(prefab);
                var scopeObject = new GameObject("Async pool scope");
                GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
                var factory = new AsyncTrackingFactory();
                scope.RegisterAsyncFactory(factory);

                System.Threading.Tasks.Task<GameObjectPoolHandle> first =
                    AwaitRegistration(scope, definition);
                System.Threading.Tasks.Task<GameObjectPoolHandle> second =
                    AwaitRegistration(scope, definition);
                GameObjectPoolHandle[] handles =
                    await System.Threading.Tasks.Task.WhenAll(first, second);

                Assert.That(handles[0], Is.SameAs(handles[1]));
                Assert.That(factory.CreateCount, Is.EqualTo(1));

                Object.Destroy(scopeObject);
                Object.Destroy(definition);
                Object.Destroy(prefab);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator ScopeCallback_DeliversOneSuccessfulResult()
        {
            async Awaitable TestImplementation()
            {
                var prefab = new GameObject("Callback pool prefab");
                prefab.SetActive(false);
                TestDefinition definition = CreateDefinition(prefab);
                var scopeObject = new GameObject("Callback pool scope");
                GameObjectPoolScope scope = scopeObject.AddComponent<GameObjectPoolScope>();
                PoolRegistrationResult result = default;
                int callbackCount = 0;

                await scope.RegisterAsync(
                    definition,
                    callbackResult =>
                    {
                        callbackCount++;
                        result = callbackResult;
                    });

                Assert.That(callbackCount, Is.EqualTo(1));
                Assert.That(result.IsSucceeded, Is.True);
                Assert.That(result.Handle, Is.Not.Null);

                Object.Destroy(scopeObject);
                Object.Destroy(definition);
                Object.Destroy(prefab);
            }

            return TestImplementation();
        }

        private static async System.Threading.Tasks.Task<GameObjectPoolHandle>
            AwaitRegistration(
                GameObjectPoolScope scope,
                GameObjectPoolDefinition definition)
        {
            return await scope.RegisterAsync(definition);
        }

        private static TestDefinition CreateDefinition(
            GameObject prefab,
            DestroyedInstancePolicy policy = DestroyedInstancePolicy.WarnAndReplace)
        {
            TestDefinition definition = ScriptableObject.CreateInstance<TestDefinition>();
            definition.Configuration = new UnityGameObjectPoolConfiguration(
                prefab,
                defaultCapacity: 1,
                maxInactiveCount: 4,
                destroyedInstancePolicy: policy);
            return definition;
        }

        private static UnityGameObjectPool CreatePool(
            TestDefinition definition)
        {
            return new UnityGameObjectPool(
                (UnityGameObjectPoolConfiguration)definition.CreateConfiguration());
        }

    }
}
