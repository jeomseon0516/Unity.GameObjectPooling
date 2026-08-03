using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Contracts;
using Jeomseon.GameObjectPooling.Definitions;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Lifetimes;
using Jeomseon.GameObjectPooling.Registrations;
using Jeomseon.GameObjectPooling.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jeomseon.GameObjectPooling.Scopes
{
    /// <summary>
    /// Owns registered pools and applies their lifetime policies. Definitions and runtime
    /// configurations both become registrations; consumers receive runtime handles.
    /// 등록된 풀을 소유하고 수명 정책을 적용합니다. Definition과 런타임 Configuration은
    /// 모두 Registration이 되며 소비자는 런타임 Handle을 받습니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameObjectPoolScope : MonoBehaviour
    {
        [SerializeField] private GameObjectPoolCatalog _catalog;
        [SerializeField] private GameObjectPoolDefinition _defaultDefinition;
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _dontDestroyOnLoad;

        private readonly Dictionary<object, GameObjectPoolHandle> _sharedHandles = new();
        private readonly HashSet<GameObjectPoolHandle> _handles = new();
        private readonly List<IPoolLifetimeHandler> _lifetimeHandlers = new();
        private readonly Dictionary<GameObjectPoolHandle, IPoolLifetimeHandler>
            _handleLifetimeHandlers = new();
        private readonly Dictionary<object, Task<GameObjectPoolHandle>>
            _pendingSharedRegistrations = new();
        private GameObjectPoolService _service;
        private CancellationTokenSource _shutdownCancellation;
        private GameObjectPoolHandle _defaultHandle;
        private GameObjectPoolHandleCollection _handleView;
        private Action<GameObjectPoolHandle> _releaseHandle;
        private bool _initialized;

        /// <summary>
        /// Gets whether this scope has initialized its service and catalog.
        /// 이 Scope가 Service와 Catalog를 초기화했는지 가져옵니다.
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Gets a read-only view of every runtime handle owned by this scope. The collection
        /// is intended for diagnostics and must not be used as a registration key store.
        /// 이 Scope가 소유한 모든 런타임 Handle의 읽기 전용 보기를 가져옵니다. 이 컬렉션은
        /// 진단용이며 등록 키 저장소로 사용하면 안 됩니다.
        /// </summary>
        public IReadOnlyCollection<GameObjectPoolHandle> Handles =>
            _handleView ??= new GameObjectPoolHandleCollection(_handles);

        /// <summary>
        /// Gets the configured default handle, resolving it lazily when required.
        /// 구성된 기본 Handle을 가져오며 필요한 경우 지연 조회합니다.
        /// </summary>
        public GameObjectPoolHandle DefaultHandle
        {
            get
            {
                Initialize();
                if (_defaultHandle != null && _defaultHandle.IsValid) return _defaultHandle;
                if (_defaultDefinition == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(GameObjectPoolScope)} requires a default Definition or " +
                        "an explicitly assigned default registration.");
                }

                _defaultHandle = Register(_defaultDefinition);
                return _defaultHandle;
            }
        }

        /// <summary>
        /// Gets the configured default handle asynchronously. This is the high-level entry
        /// point when the default Definition requires an asynchronous factory.
        /// 구성된 기본 Handle을 비동기로 가져옵니다. 기본 Definition이 비동기 Factory를
        /// 요구할 때 사용하는 고수준 진입점입니다.
        /// </summary>
        public async Awaitable<GameObjectPoolHandle> GetDefaultHandleAsync(
            CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            if (_defaultHandle != null && _defaultHandle.IsValid) return _defaultHandle;
            if (_defaultDefinition == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameObjectPoolScope)} requires a default Definition or " +
                    "an explicitly assigned default registration.");
            }

            _defaultHandle = await RegisterAsync(_defaultDefinition, cancellationToken);
            return _defaultHandle;
        }

        private void Awake()
        {
            if (_dontDestroyOnLoad && Application.isPlaying)
            {
                if (transform.parent != null)
                {
                    throw new InvalidOperationException(
                        $"A persistent {nameof(GameObjectPoolScope)} must be on a root GameObject. / " +
                        $"영속 {nameof(GameObjectPoolScope)}는 루트 GameObject에 있어야 합니다.");
                }

                DontDestroyOnLoad(gameObject);
            }

            if (_initializeOnAwake) Initialize();
        }

        /// <summary>
        /// Initializes the owned service and preloads the optional catalog.
        /// 소유 Service를 초기화하고 선택적 Catalog를 미리 등록합니다.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            InitializeCore();
            try
            {
                LoadCatalogEntries(_catalog);
            }
            catch
            {
                Shutdown();
                throw;
            }
        }

        /// <summary>
        /// Initializes the service and asynchronously preloads the optional catalog. Async
        /// factories are preferred and synchronous factories remain a valid fallback.
        /// Service를 초기화하고 선택적 Catalog를 비동기로 미리 등록합니다. 비동기 Factory를
        /// 우선하며 동기 Factory도 유효한 fallback으로 유지합니다.
        /// </summary>
        public async Awaitable InitializeAsync(
            CancellationToken cancellationToken = default)
        {
            if (_initialized) return;
            InitializeCore();
            try
            {
                await LoadCatalogEntriesAsync(_catalog, cancellationToken);
            }
            catch
            {
                Shutdown();
                throw;
            }
        }

        /// <summary>
        /// Registers a custom pool factory. Later factories take precedence.
        /// 사용자 풀 Factory를 등록합니다. 나중에 등록한 Factory가 우선합니다.
        /// </summary>
        public void RegisterFactory(IGameObjectPoolFactory factory)
        {
            _service ??= new GameObjectPoolService(transform);
            _service.RegisterFactory(factory);
        }

        /// <summary>
        /// Registers a custom asynchronous pool factory. Later factories take precedence.
        /// 사용자 비동기 풀 Factory를 등록합니다. 나중에 등록한 Factory가 우선합니다.
        /// </summary>
        public void RegisterAsyncFactory(IAsyncGameObjectPoolFactory factory)
        {
            _service ??= new GameObjectPoolService(transform);
            _service.RegisterAsyncFactory(factory);
        }

        /// <summary>
        /// Registers and transfers ownership of a custom lifetime handler to this scope.
        /// Later handlers take precedence when multiple handlers support one configuration.
        /// 사용자 수명 Handler를 등록하고 소유권을 이 Scope로 이전합니다. 여러 Handler가 한
        /// Configuration을 지원하면 나중에 등록한 Handler가 우선합니다.
        /// </summary>
        public void RegisterLifetimeHandler(IPoolLifetimeHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (_lifetimeHandlers.Contains(handler))
            {
                throw new ArgumentException(
                    "The lifetime handler is already registered.", nameof(handler));
            }

            EnsureBuiltInLifetimeHandler();
            _lifetimeHandlers.Add(handler);
        }

        /// <summary>
        /// Registers a serialized Definition. The same Definition resolves one shared handle
        /// inside this scope.
        /// 직렬화된 Definition을 등록합니다. 같은 Definition은 이 Scope 안에서 하나의 공유
        /// Handle로 조회됩니다.
        /// </summary>
        public GameObjectPoolHandle Register(GameObjectPoolDefinition definition)
        {
            return definition == null
                ? throw new ArgumentNullException(nameof(definition))
                : Register(definition.CreateRegistration());
        }

        /// <summary>
        /// Asynchronously registers a serialized Definition. Concurrent registration of the
        /// same Definition shares one creation operation within this scope.
        /// 직렬화된 Definition을 비동기로 등록합니다. 같은 Definition의 동시 등록은 이
        /// Scope 안에서 하나의 생성 작업을 공유합니다.
        /// </summary>
        public Awaitable<GameObjectPoolHandle> RegisterAsync(
            GameObjectPoolDefinition definition,
            CancellationToken cancellationToken = default)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return RegisterAsync(definition.CreateRegistration(), cancellationToken);
        }

        /// <summary>
        /// Tries to get the shared handle already registered for a Definition.
        /// Definition에 이미 등록된 공유 Handle을 가져옵니다.
        /// </summary>
        public bool TryGet(
            GameObjectPoolDefinition definition,
            out GameObjectPoolHandle handle)
        {
            if (definition == null)
            {
                handle = null;
                return false;
            }

            return _sharedHandles.TryGetValue(definition, out handle) && handle.IsValid;
        }

        /// <summary>
        /// Tries to get the currently assigned default handle without resolving a Definition.
        /// Definition을 새로 조회하지 않고 현재 지정된 기본 Handle을 가져옵니다.
        /// </summary>
        public bool TryGetDefault(out GameObjectPoolHandle handle)
        {
            handle = _defaultHandle;
            return handle != null && handle.IsValid;
        }

        /// <summary>
        /// Registers runtime configurations as an independent anonymous pool.
        /// 런타임 Configuration을 독립된 익명 풀로 등록합니다.
        /// </summary>
        public GameObjectPoolHandle Register(
            IGameObjectPoolConfiguration poolConfiguration,
            IPoolLifetimeConfiguration lifetimeConfiguration,
            string name = null)
        {
            return Register(new GameObjectPoolRegistration(
                poolConfiguration,
                lifetimeConfiguration,
                name));
        }

        /// <summary>
        /// Asynchronously registers runtime configurations as an independent anonymous pool.
        /// 런타임 Configuration을 독립된 익명 풀로 비동기 등록합니다.
        /// </summary>
        public Awaitable<GameObjectPoolHandle> RegisterAsync(
            IGameObjectPoolConfiguration poolConfiguration,
            IPoolLifetimeConfiguration lifetimeConfiguration,
            string name = null,
            CancellationToken cancellationToken = default)
        {
            return RegisterAsync(
                new GameObjectPoolRegistration(
                    poolConfiguration,
                    lifetimeConfiguration,
                    name),
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously registers pool creation input and returns its runtime identity.
        /// 풀 생성 입력을 비동기로 등록하고 런타임 식별자를 반환합니다.
        /// </summary>
        public async Awaitable<GameObjectPoolHandle> RegisterAsync(
            GameObjectPoolRegistration registration,
            CancellationToken cancellationToken = default)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            await InitializeAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            object identity = registration.SharedIdentity;
            if (identity == null)
            {
                return await CreateAndCommitAsync(registration, cancellationToken);
            }

            if (_sharedHandles.TryGetValue(identity, out GameObjectPoolHandle existing) &&
                existing.IsValid)
            {
                RegisterExistingLifetime(existing, registration.LifetimeConfiguration);
                return existing;
            }

            bool ownsCreation = false;
            if (!_pendingSharedRegistrations.TryGetValue(
                    identity,
                    out Task<GameObjectPoolHandle> pending))
            {
                ownsCreation = true;
                pending = CreateSharedRegistrationTask(
                    identity,
                    registration,
                    _shutdownCancellation.Token);
                if (!pending.IsCompleted)
                {
                    _pendingSharedRegistrations.Add(identity, pending);
                }
            }

            GameObjectPoolHandle handle = await WaitForCallerAsync(
                pending,
                cancellationToken);
            if (!ownsCreation)
            {
                RegisterExistingLifetime(handle, registration.LifetimeConfiguration);
            }
            return handle;
        }

        /// <summary>
        /// Registers pool creation input and returns its runtime identity.
        /// 풀 생성 입력을 등록하고 런타임 식별자를 반환합니다.
        /// </summary>
        public GameObjectPoolHandle Register(GameObjectPoolRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            Initialize();
            object identity = registration.SharedIdentity;
            if (identity != null &&
                _sharedHandles.TryGetValue(identity, out GameObjectPoolHandle existing) &&
                existing.IsValid)
            {
                RegisterExistingLifetime(existing, registration.LifetimeConfiguration);
                return existing;
            }

            IPoolLifetimeHandler lifetimeHandler = ResolveLifetimeHandler(
                registration.LifetimeConfiguration);
            lifetimeHandler.Validate(registration.LifetimeConfiguration);
            IGameObjectPool pool = _service.CreatePool(registration.PoolConfiguration);
            return CommitRegistration(registration, lifetimeHandler, pool);
        }

        private GameObjectPoolHandle CommitRegistration(
            GameObjectPoolRegistration registration,
            IPoolLifetimeHandler lifetimeHandler,
            IGameObjectPool pool)
        {
            var lifetimeContext = new PoolLifetimeRegistrationContext(
                SceneManager.GetActiveScene().handle,
                _releaseHandle);
            object identity = registration.SharedIdentity;
            var handle = new GameObjectPoolHandle(registration.Name, pool);
            try
            {
                _handles.Add(handle);
                if (identity != null) _sharedHandles.Add(identity, handle);
                lifetimeHandler.Register(
                    handle,
                    registration.LifetimeConfiguration,
                    lifetimeContext);
                _handleLifetimeHandlers.Add(handle, lifetimeHandler);
                return handle;
            }
            catch
            {
                TryUnregisterLifetimeHandler(lifetimeHandler, handle);
                _handleLifetimeHandlers.Remove(handle);
                _handles.Remove(handle);
                if (identity != null) _sharedHandles.Remove(identity);
                _service.ReleasePool(pool);
                handle.Invalidate();
                throw;
            }
        }

        private void RegisterExistingLifetime(
            GameObjectPoolHandle handle,
            IPoolLifetimeConfiguration configuration)
        {
            IPoolLifetimeHandler handler = _handleLifetimeHandlers[handle];
            handler.Validate(configuration);
            var context = new PoolLifetimeRegistrationContext(
                SceneManager.GetActiveScene().handle,
                _releaseHandle);
            handler.Register(handle, configuration, context);
        }

        /// <summary>
        /// Assigns the default handle used by the concise Spawn and Despawn API.
        /// 간결한 Spawn 및 Despawn API가 사용할 기본 Handle을 지정합니다.
        /// </summary>
        public void SetDefault(GameObjectPoolHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (!_handles.Contains(handle) || !handle.IsValid)
            {
                throw new ArgumentException(
                    "The handle is not owned by this scope.", nameof(handle));
            }

            _defaultHandle = handle;
        }

        /// <summary>
        /// Releases one handle and its owned pool.
        /// 하나의 Handle과 소유 풀을 해제합니다.
        /// </summary>
        public bool Release(GameObjectPoolHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (!_handles.Remove(handle)) return false;

            RemoveSharedIdentity(handle);
            if (_handleLifetimeHandlers.Remove(
                    handle,
                    out IPoolLifetimeHandler lifetimeHandler))
            {
                TryUnregisterLifetimeHandler(lifetimeHandler, handle);
            }
            if (ReferenceEquals(_defaultHandle, handle)) _defaultHandle = null;
            IGameObjectPool pool = handle.Pool;
            handle.Invalidate();
            return _service.ReleasePool(pool);
        }

        /// <summary>
        /// Preloads every valid Definition in a catalog.
        /// Catalog의 유효한 모든 Definition을 미리 등록합니다.
        /// </summary>
        public void LoadCatalog(GameObjectPoolCatalog catalog)
        {
            Initialize();
            LoadCatalogEntries(catalog);
        }

        /// <summary>
        /// Asynchronously preloads every valid Definition in a catalog.
        /// Catalog의 유효한 모든 Definition을 비동기로 미리 등록합니다.
        /// </summary>
        public async Awaitable LoadCatalogAsync(
            GameObjectPoolCatalog catalog,
            CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            await LoadCatalogEntriesAsync(catalog, cancellationToken);
        }

        /// <summary>
        /// Spawns from the default handle.
        /// 기본 Handle에서 생성합니다.
        /// </summary>
        public GameObject Spawn() => DefaultHandle.Spawn();

        /// <summary>
        /// Spawns from the default handle with transform options.
        /// Transform 옵션으로 기본 Handle에서 생성합니다.
        /// </summary>
        public GameObject Spawn(in PoolSpawnOptions options) => DefaultHandle.Spawn(options);

        /// <summary>
        /// Spawns a component from the default handle.
        /// 기본 Handle에서 Component를 생성합니다.
        /// </summary>
        public T Spawn<T>() where T : Component => DefaultHandle.Spawn<T>();

        /// <summary>
        /// Spawns a component from the default handle with transform options.
        /// Transform 옵션으로 기본 Handle에서 Component를 생성합니다.
        /// </summary>
        public T Spawn<T>(in PoolSpawnOptions options) where T : Component =>
            DefaultHandle.Spawn<T>(options);

        /// <summary>
        /// Returns a GameObject through the default handle.
        /// 기본 Handle을 통해 GameObject를 반환합니다.
        /// </summary>
        public void Despawn(GameObject instance) => DefaultHandle.Despawn(instance);

        /// <summary>
        /// Returns a component through the default handle.
        /// 기본 Handle을 통해 Component를 반환합니다.
        /// </summary>
        public void Despawn<T>(T component) where T : Component =>
            DefaultHandle.Despawn(component);

        /// <summary>
        /// Releases all registered pools and allows reinitialization.
        /// 등록된 모든 풀을 해제하고 다시 초기화할 수 있게 합니다.
        /// </summary>
        public void Shutdown()
        {
            _shutdownCancellation?.Cancel();
            foreach (IPoolLifetimeHandler handler in _lifetimeHandlers)
            {
                try
                {
                    handler.Dispose();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }

            foreach (GameObjectPoolHandle handle in _handles) handle.Invalidate();
            _service?.Dispose();
            _service = null;
            _handles.Clear();
            _sharedHandles.Clear();
            _handleLifetimeHandlers.Clear();
            _pendingSharedRegistrations.Clear();
            _lifetimeHandlers.Clear();
            _defaultHandle = null;
            _initialized = false;
            _shutdownCancellation?.Dispose();
            _shutdownCancellation = null;
        }

        private void InitializeCore()
        {
            _service ??= new GameObjectPoolService(transform);
            _shutdownCancellation ??= new CancellationTokenSource();
            EnsureBuiltInLifetimeHandler();
            _releaseHandle ??= handle => Release(handle);
            _initialized = true;
        }

        private void LoadCatalogEntries(GameObjectPoolCatalog catalog)
        {
            if (catalog == null) return;
            var loaded = new HashSet<GameObjectPoolDefinition>();
            foreach (GameObjectPoolDefinition definition in catalog.Definitions)
            {
                if (definition == null || !loaded.Add(definition)) continue;
                Register(definition);
            }
        }

        private async Awaitable LoadCatalogEntriesAsync(
            GameObjectPoolCatalog catalog,
            CancellationToken cancellationToken)
        {
            if (catalog == null) return;
            var loaded = new HashSet<GameObjectPoolDefinition>();
            foreach (GameObjectPoolDefinition definition in catalog.Definitions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (definition == null || !loaded.Add(definition)) continue;
                await RegisterAsync(definition, cancellationToken);
            }
        }

        private async Task<GameObjectPoolHandle> CreateSharedRegistrationTask(
            object identity,
            GameObjectPoolRegistration registration,
            CancellationToken cancellationToken)
        {
            try
            {
                return await CreateAndCommitAsync(registration, cancellationToken);
            }
            finally
            {
                _pendingSharedRegistrations.Remove(identity);
            }
        }

        private static async Task<T> WaitForCallerAsync<T>(
            Task<T> sharedTask,
            CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled || sharedTask.IsCompleted)
            {
                return await sharedTask;
            }

            var cancellation = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => cancellation.TrySetCanceled(cancellationToken));
            Task<T> completed = await Task.WhenAny(sharedTask, cancellation.Task);
            return await completed;
        }

        private async Awaitable<GameObjectPoolHandle> CreateAndCommitAsync(
            GameObjectPoolRegistration registration,
            CancellationToken cancellationToken)
        {
            IPoolLifetimeHandler lifetimeHandler = ResolveLifetimeHandler(
                registration.LifetimeConfiguration);
            lifetimeHandler.Validate(registration.LifetimeConfiguration);
            GameObjectPoolService service = _service;
            IGameObjectPool pool = await service.CreatePoolAsync(
                registration.PoolConfiguration,
                cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                service.ReleasePool(pool);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return CommitRegistration(registration, lifetimeHandler, pool);
        }

        private void EnsureBuiltInLifetimeHandler()
        {
            if (_lifetimeHandlers.Count != 0) return;
            _lifetimeHandlers.Add(new BuiltInPoolLifetimeHandler(
                this,
                _dontDestroyOnLoad));
        }

        private IPoolLifetimeHandler ResolveLifetimeHandler(
            IPoolLifetimeConfiguration configuration)
        {
            for (int i = _lifetimeHandlers.Count - 1; i >= 0; i--)
            {
                IPoolLifetimeHandler handler = _lifetimeHandlers[i];
                if (handler.CanHandle(configuration)) return handler;
            }

            throw new NotSupportedException(
                $"No lifetime handler supports {configuration.GetType().FullName}.");
        }

        private void RemoveSharedIdentity(GameObjectPoolHandle handle)
        {
            object identityToRemove = null;
            foreach (KeyValuePair<object, GameObjectPoolHandle> pair in _sharedHandles)
            {
                if (!ReferenceEquals(pair.Value, handle)) continue;
                identityToRemove = pair.Key;
                break;
            }

            if (identityToRemove != null) _sharedHandles.Remove(identityToRemove);
        }

        private void TryUnregisterLifetimeHandler(
            IPoolLifetimeHandler handler,
            GameObjectPoolHandle handle)
        {
            try
            {
                handler.Unregister(handle);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void OnDestroy() => Shutdown();
    }
}
