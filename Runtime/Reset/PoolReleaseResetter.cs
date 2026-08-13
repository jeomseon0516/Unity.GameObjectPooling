using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Reset
{
    /// <summary>
    /// Restores attributed component members using metadata cached once per component type.
    /// Component 타입별로 한 번 캐시한 메타데이터를 사용하여 Attribute 멤버를 복원합니다.
    /// </summary>
    internal static class PoolReleaseResetter
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, ResetMember[]> _cache = new();

        internal static void Reset(GameObject gameObject)
        {
            if (gameObject == null) return;

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component != null) Reset(component);
            }
        }

        private static void Reset(Component component)
        {
            Type type = component.GetType();
            if (!_cache.TryGetValue(type, out ResetMember[] members))
            {
                members = BuildMembers(type);
                _cache.Add(type, members);
            }

            foreach (ResetMember member in members) member.Reset(component);
        }

        private static ResetMember[] BuildMembers(Type type)
        {
            var members = new List<ResetMember>();
            foreach (FieldInfo field in type.GetFields(MemberFlags))
            {
                ResetOnPoolReleaseAttribute attribute =
                    field.GetCustomAttribute<ResetOnPoolReleaseAttribute>();
                if (attribute == null || field.IsStatic || field.IsInitOnly || field.IsLiteral)
                {
                    continue;
                }

                members.Add(new ResetMember(
                    field,
                    ResolveValue(field.FieldType, attribute.Value)));
            }

            foreach (PropertyInfo property in type.GetProperties(MemberFlags))
            {
                ResetOnPoolReleaseAttribute attribute =
                    property.GetCustomAttribute<ResetOnPoolReleaseAttribute>();
                MethodInfo setter = property.SetMethod;
                if (attribute == null || setter == null || setter.IsStatic ||
                    property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                members.Add(new ResetMember(
                    property,
                    ResolveValue(property.PropertyType, attribute.Value)));
            }

            return members.ToArray();
        }

        private static object ResolveValue(Type memberType, object configuredValue)
        {
            if (configuredValue == null)
            {
                return memberType.IsValueType ? Activator.CreateInstance(memberType) : null;
            }

            if (!memberType.IsInstanceOfType(configuredValue))
            {
                throw new InvalidOperationException(
                    $"Pool reset value of type {configuredValue.GetType().FullName} cannot " +
                    $"be assigned to {memberType.FullName}.");
            }

            return configuredValue;
        }

        private readonly struct ResetMember
        {
            private readonly FieldInfo _field;
            private readonly PropertyInfo _property;
            private readonly object _value;

            internal ResetMember(FieldInfo field, object value)
            {
                _field = field;
                _property = null;
                _value = value;
            }

            internal ResetMember(PropertyInfo property, object value)
            {
                _field = null;
                _property = property;
                _value = value;
            }

            internal void Reset(Component component)
            {
                if (_field != null) _field.SetValue(component, _value);
                else _property.SetValue(component, _value);
            }
        }
    }
}
