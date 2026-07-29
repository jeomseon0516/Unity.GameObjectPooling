using System;
using System.Reflection;
using UnityEngine;

namespace Jeomseon.ObjectPool
{
    // TODO(performance): 컴포넌트 타입별 초기화 메타데이터를 캐싱하거나 리플렉션을
    // 명시적인 초기화 전략으로 교체해야 합니다. 현재는 반환할 때마다 모든 필드와
    // 프로퍼티를 탐색합니다. ScriptableObject 풀 정의에서 초기화 전략을 선택할 수 있어야 합니다.
    internal static class PoolInitializer
    {
        internal static void Initialize(object obj)
        {
            if (obj == null) return;

            // Component이면 자기 자신만 초기화
            if (obj is Component comp)
            {
                initializeComponent(comp);
            }
            // GameObject이면 붙어 있는 모든 Component 초기화
            else if (obj is GameObject go)
            {
                foreach (var c in go.GetComponents<Component>())
                    initializeComponent(c);
            }
        }

        private static void initializeComponent(Component comp)
        {
            var type = comp.GetType();

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<PoolInitAttribute>();
                if (attr == null) continue;

                if (!prop.CanWrite) continue; // 읽기 전용이면 패스

                if (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string))
                    prop.SetValue(comp, attr.DefaultValue ?? Activator.CreateInstance(prop.PropertyType));
                else if (typeof(Component).IsAssignableFrom(prop.PropertyType))
                {
                    var mono = comp as MonoBehaviour;
                    if (mono != null)
                    {
                        prop.SetValue(comp, null);
                    }
                }
            }

            // 필드 처리 (기존 방식)
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<PoolInitAttribute>();
                if (attr == null) continue;

                if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                    field.SetValue(comp, attr.DefaultValue ?? Activator.CreateInstance(field.FieldType));
                else if (typeof(Component).IsAssignableFrom(field.FieldType))
                {
                    var mono = comp as MonoBehaviour;
                    if (mono != null)
                    {
                        field.SetValue(comp, null);
                    }
                }
            }
        }
    }
}
