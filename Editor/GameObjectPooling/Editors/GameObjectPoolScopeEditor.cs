using System.Collections.Generic;
using Jeomseon.GameObjectPooling.Configurations;
using Jeomseon.GameObjectPooling.Contracts;
using Jeomseon.GameObjectPooling.Definitions;
using Jeomseon.GameObjectPooling.Handles;
using Jeomseon.GameObjectPooling.Scopes;
using UnityEditor;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Editors
{
    [CustomEditor(typeof(GameObjectPoolScope))]
    internal sealed class GameObjectPoolScopeEditor : UnityEditor.Editor
    {
        private string _lastLoggedValidationMessage;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            var scope = (GameObjectPoolScope)target;
            SerializedProperty persistent = serializedObject.FindProperty("dontDestroyOnLoad");
            SerializedProperty catalogProperty = serializedObject.FindProperty("catalog");
            SerializedProperty definitionProperty = serializedObject.FindProperty("defaultDefinition");
            string validationMessage = GetValidationMessage(
                scope,
                persistent.boolValue,
                catalogProperty.objectReferenceValue as GameObjectPoolCatalog,
                definitionProperty.objectReferenceValue as GameObjectPoolDefinition);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Error);
                LogValidationErrorOnce(scope, validationMessage);
            }
            else
            {
                _lastLoggedValidationMessage = null;
            }

            var catalog = catalogProperty.objectReferenceValue as GameObjectPoolCatalog;
            var defaultDefinition =
                definitionProperty.objectReferenceValue as GameObjectPoolDefinition;
            if (Application.isPlaying && scope.IsInitialized)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Runtime Statistics / 런타임 통계",
                    EditorStyles.boldLabel);
                var handles = new List<GameObjectPoolHandle>(scope.Handles);
                handles.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
                scope.TryGetDefault(out GameObjectPoolHandle defaultHandle);
                foreach (GameObjectPoolHandle handle in handles)
                {
                    string defaultSuffix = ReferenceEquals(handle, defaultHandle)
                        ? " (Default / 기본)"
                        : string.Empty;
                    EditorGUILayout.LabelField(
                        handle.Name + defaultSuffix,
                        EditorStyles.boldLabel);
                    if (!handle.TryGetStatistics(out PoolStatistics statistics))
                    {
                        EditorGUILayout.LabelField(
                            "Diagnostics unavailable / 진단 정보 없음");
                        continue;
                    }

                    EditorGUILayout.LabelField(
                        $"Active {statistics.CountActive} | Inactive {statistics.CountInactive} | " +
                        $"Created {statistics.CreatedCount} | Destroyed {statistics.DestroyedCount}");
                    EditorGUILayout.LabelField(
                        $"Released {statistics.ReleasedCount} | Invalid {statistics.InvalidReleaseCount} | " +
                        $"Capacity Discarded {statistics.CapacityDiscardedCount}");
                }

                Repaint();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool RequiresPersistentScope(GameObjectPoolDefinition definition)
        {
            return definition?.CreateLifetimeConfiguration() is
                PoolLifetimeConfiguration
                {
                    Lifetime: PoolLifetime.Application
                };
        }

        private static string GetValidationMessage(
            GameObjectPoolScope scope,
            bool persistent,
            GameObjectPoolCatalog catalog,
            GameObjectPoolDefinition defaultDefinition)
        {
            if (persistent && scope.transform.parent != null)
            {
                return "A persistent scope must be a root GameObject. / " +
                       "영속 Scope는 루트 GameObject여야 합니다.";
            }

            if (persistent) return null;
            if (RequiresPersistentScope(defaultDefinition))
            {
                return $"Application lifetime Definition '{defaultDefinition.name}' requires " +
                       "Dont Destroy On Load. / Application 수명 Definition " +
                       $"'{defaultDefinition.name}'에는 Dont Destroy On Load 설정이 필요합니다.";
            }

            if (catalog == null) return null;
            foreach (GameObjectPoolDefinition definition in catalog.Definitions)
            {
                if (definition == null || definition == defaultDefinition ||
                    !RequiresPersistentScope(definition))
                {
                    continue;
                }

                return $"Catalog Definition '{definition.name}' uses Application lifetime and " +
                       "requires Dont Destroy On Load. / Catalog Definition " +
                       $"'{definition.name}'은 Application 수명이므로 Dont Destroy On Load " +
                       "설정이 필요합니다.";
            }

            return null;
        }

        private void LogValidationErrorOnce(
            GameObjectPoolScope scope,
            string validationMessage)
        {
            if (_lastLoggedValidationMessage == validationMessage) return;

            _lastLoggedValidationMessage = validationMessage;
            Debug.LogError(
                $"[{nameof(GameObjectPoolScope)}] {validationMessage}",
                scope);
        }
    }
}
