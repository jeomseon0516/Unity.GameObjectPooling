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

    // TODO(architecture): 전역 정적 풀을 ScriptableObject 에셋 기반의 유니티 친화적인
    // 풀 서비스로 교체해야 합니다. 에셋에서 프리팹, 초기 용량, 최대 용량, 용량 초과 정책,
    // 씬 수명 설정을 관리하고, 코드 전용 API는 선택 가능한 저수준 구현으로 유지합니다.
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
    
    // TODO(architecture): 문자열 키를 ScriptableObject 에셋에 저장되는 안정적인 풀 정의
    // 또는 타입 기반 식별자로 교체해야 합니다. 키 충돌을 방지하고 Inspector에서
    // 풀 설정을 쉽게 찾고 편집할 수 있어야 합니다.
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
    
    // TODO(architecture): Generic, Keyed, GenericKeyed로 나뉜 세 개의 전역 저장소를
    // 각각 유지하지 않고 하나의 설정 가능한 풀 서비스로 통합해야 합니다.
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
