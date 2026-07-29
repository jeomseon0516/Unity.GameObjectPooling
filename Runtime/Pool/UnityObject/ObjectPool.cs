using System.Collections.Generic;
using UnityEngine;

namespace Jeomseon.ObjectPool
{
    public interface IPooledObject : IPopHandler, IReturnHandler {}

    public interface IPopHandler
    {
        void OnPopFromPool();
    }

    public interface IReturnHandler
    {
        void OnReturnToPool();
    }

    // TODO(architecture): Replace the global static pool with a Unity-friendly pool
    // service backed by ScriptableObject assets. The asset should own prefab,
    // initial capacity, maximum capacity, overflow policy, and scene lifetime settings.
    // Keep a code-only API as an optional low-level implementation.
    public static class GenericObjectPool<T> where T : Component
    {
        private static readonly Stack<T> _pool = new();
        private static GameObject _poolParent = null;

        private static GameObject getPoolParent()
        {
            if (_poolParent == null)
            {
                _poolParent = new(nameof(GenericObjectPool<T>));
                if (Application.isPlaying)
                {
                    Object.DontDestroyOnLoad(_poolParent);
                }
            }

            return _poolParent;
        }

        public static bool TryPop(out T genericComponent)
        {
            if (_pool.TryPop(out genericComponent) && genericComponent)
            {
                onPop(genericComponent);
                return true;
            }
            return false;
        }

        public static T Pop()
        {
            if (!_pool.TryPop(out T genericComponent) || !genericComponent) return null;

            onPop(genericComponent);
            return genericComponent;
        }

        public static void Return(T genericComponent)
        {
            if (!genericComponent) return;
            
            if (genericComponent is IReturnHandler pooledObject)
            {
                pooledObject.OnReturnToPool();
            }
            
            genericComponent.gameObject.SetActive(false);
            genericComponent.transform.SetParent(getPoolParent().transform, false);
            PoolInitializer.Initialize(genericComponent);
            
            _pool.Push(genericComponent);
        }

        public static void ReleaseAllObject()
        {
            foreach (T component in _pool)
            {
                if (component)
                {
                    Object.Destroy(component.gameObject);
                }
            }

            _pool.Clear();
        }

        private static void onPop(T genericComponent)
        {
            genericComponent.transform.SetParent(null, false);
            genericComponent.gameObject.SetActive(true);
            if (genericComponent is IPopHandler pooledObject)
            {
                pooledObject.OnPopFromPool();
            }
        }
    }
    
    // TODO(architecture): Replace string keys with stable pool definitions or typed
    // identifiers stored in ScriptableObject assets. This should prevent key collisions
    // and make pool configuration discoverable and editable in the Inspector.
    public static class KeyedObjectPool
    {
        private static readonly Dictionary<string, Stack<GameObject>> _pool = new();
        private static GameObject _poolParent = null;

        private static GameObject getPoolParent()
        {
            if (_poolParent == null)
            {
                _poolParent = new(nameof(KeyedObjectPool));
                if (Application.isPlaying)
                {
                    Object.DontDestroyOnLoad(_poolParent);
                }
            }

            return _poolParent;
        }

        public static bool TryPop(string key, out GameObject pooledObject)
        {
            pooledObject = null;
            
            if (_pool.TryGetValue(key, out Stack<GameObject> stack))
            {
                while (stack.TryPop(out pooledObject))
                {
                    if (!pooledObject) continue;

                    onPop(pooledObject);
                    return true;
                }
            }

            return false;
        }

        public static GameObject Pop(string key)
        {
            if (!_pool.TryGetValue(key, out Stack<GameObject> stack) || stack.Count <= 0) return null;

            while (stack.TryPop(out GameObject pooledObject))
            {
                if (!pooledObject) continue;

                onPop(pooledObject);
                return pooledObject;
            }

            return null;
        }
        
        public static void Return(string key, GameObject objectToReturn)
        {
            if (string.IsNullOrEmpty(key) || !objectToReturn) return;
            
            if (!_pool.TryGetValue(key, out Stack<GameObject> stack))
            {
                stack = new();
                _pool.Add(key, stack);
            }

            foreach (IReturnHandler pooledComponent in objectToReturn.GetComponents<IReturnHandler>())
            {
                pooledComponent.OnReturnToPool();
            }
            
            objectToReturn.SetActive(false);
            objectToReturn.transform.SetParent(getPoolParent().transform, false);
            PoolInitializer.Initialize(objectToReturn);

            stack.Push(objectToReturn);
        }
        
        public static void ReleaseAllObject()
        {
            foreach (Stack<GameObject> stack in _pool.Values)
            {
                foreach (GameObject pooledObject in stack)
                {
                    if (pooledObject)
                    {
                        Object.Destroy(pooledObject);
                    }
                }
            }

            _pool.Clear();
        }
        
        private static void onPop(GameObject pooledObject)
        {
            pooledObject.transform.SetParent(null, false);
            pooledObject.SetActive(true);
            foreach (IPopHandler pooledComponent in pooledObject.GetComponents<IPopHandler>())
            {
                pooledComponent.OnPopFromPool();
            }
        }
    }
    
    // TODO(architecture): Consolidate the generic, keyed, and generic-keyed variants
    // behind one configurable pool service instead of maintaining three global stores.
    public static class GenericKeyedObjectPool<T> where T : Component
    {
        private static readonly Dictionary<string, Stack<T>> _pool = new();
        private static GameObject _poolParent = null;

        private static GameObject getPoolParent()
        {
            if (_poolParent == null)
            {
                _poolParent = new(nameof(GenericKeyedObjectPool<T>));
                if (Application.isPlaying)
                {
                    Object.DontDestroyOnLoad(_poolParent);
                }
            }

            return _poolParent;
        }

        public static bool TryPop(string key, out T genericComponent)
        {
            genericComponent = null;
            if (_pool.TryGetValue(key, out Stack<T> stack))
            {
                while (stack.TryPop(out genericComponent))
                {
                    if (!genericComponent) continue;

                    onPop(genericComponent);
                    return true;
                }
            }
            
            return false;
        }

        public static T Pop(string key)
        {
            if (!_pool.TryGetValue(key, out Stack<T> stack) || stack.Count <= 0) return null;

            while (stack.TryPop(out T genericComponent))
            {
                if (!genericComponent) continue;

                onPop(genericComponent);
                return genericComponent;
            }

            return null;
        }
        
        public static void Return(string key, T objectToReturn)
        {
            if (string.IsNullOrEmpty(key) || !objectToReturn) return;
            
            if (!_pool.TryGetValue(key, out Stack<T> stack))
            {
                stack = new();
                _pool.Add(key, stack);
            }

            if (objectToReturn is IReturnHandler pooledObject)
            {
                pooledObject.OnReturnToPool();
            }
            
            objectToReturn.gameObject.SetActive(false);
            objectToReturn.transform.SetParent(getPoolParent().transform, false);
            PoolInitializer.Initialize(objectToReturn);

            stack.Push(objectToReturn);
        }
        
        public static void ReleaseAllObject()
        {
            foreach (Stack<T> stack in _pool.Values)
            {
                foreach (T pooledObject in stack)
                {
                    if (pooledObject)
                    {
                        Object.Destroy(pooledObject.gameObject);
                    }
                }
            }

            _pool.Clear();
        }
        
        private static void onPop(T genericComponent)
        {
            genericComponent.transform.SetParent(null, false);
            genericComponent.gameObject.SetActive(true);

            if (genericComponent is IPopHandler pooledObject)
            {
                pooledObject.OnPopFromPool();
            }
        }
    }
}
