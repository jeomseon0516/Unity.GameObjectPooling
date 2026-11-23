using System.Collections.Generic;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using Jeomseon.Unity.GameObjectPooling.Contracts;
using Jeomseon.Unity.GameObjectPooling.Definitions;
using Jeomseon.Unity.GameObjectPooling.Handles;
using Jeomseon.Unity.GameObjectPooling.Scopes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.GameObjectPooling.Editor.Editors
{
    [CustomEditor(typeof(GameObjectPoolScope))]
    internal sealed class GameObjectPoolScopeEditor : UnityEditor.Editor
    {
        // .. IMGUI 경로는 Play Mode 통계 표시 중 매 OnInspectorGUI에서 Repaint를 요청합니다. UI Toolkit
        // .. 경로는 같은 연속 갱신을 Inspector가 표시되는 동안 짧은 주기의 scheduler로 대신합니다.
        private const long RefreshIntervalMs = 100;

        private readonly List<GameObjectPoolHandle> _sortedHandles = new();
        private string _lastLoggedValidationMessage;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var validationBox = new HelpBox(string.Empty, HelpBoxMessageType.Error)
            {
                style = { display = DisplayStyle.None }
            };
            root.Add(validationBox);

            var statisticsRoot = new VisualElement
            {
                style = { display = DisplayStyle.None }
            };
            root.Add(statisticsRoot);

            void Refresh()
            {
                if (target == null) return;

                serializedObject.UpdateIfRequiredOrScript();
                RefreshValidation(validationBox);
                RefreshRuntimeStatistics(statisticsRoot);
            }

            Refresh();
            root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            root.schedule.Execute(Refresh).Every(RefreshIntervalMs);
            return root;
        }

        private void RefreshValidation(HelpBox validationBox)
        {
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
                validationBox.text = validationMessage;
                validationBox.style.display = DisplayStyle.Flex;
                LogValidationErrorOnce(scope, validationMessage);
            }
            else
            {
                validationBox.style.display = DisplayStyle.None;
                _lastLoggedValidationMessage = null;
            }
        }

        private void RefreshRuntimeStatistics(VisualElement statisticsRoot)
        {
            var scope = (GameObjectPoolScope)target;
            statisticsRoot.Clear();
            if (!Application.isPlaying || !scope.IsInitialized)
            {
                statisticsRoot.style.display = DisplayStyle.None;
                return;
            }

            statisticsRoot.style.display = DisplayStyle.Flex;
            statisticsRoot.Add(new VisualElement { style = { height = 6f } });
            statisticsRoot.Add(CreateBoldLabel("Runtime Statistics / 런타임 통계"));
            _sortedHandles.Clear();
            _sortedHandles.AddRange(scope.Handles);
            _sortedHandles.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
            scope.TryGetDefault(out GameObjectPoolHandle defaultHandle);
            foreach (GameObjectPoolHandle handle in _sortedHandles)
            {
                string defaultSuffix = ReferenceEquals(handle, defaultHandle)
                    ? " (Default / 기본)"
                    : string.Empty;
                statisticsRoot.Add(CreateBoldLabel(handle.Name + defaultSuffix));
                if (!handle.TryGetStatistics(out PoolStatistics statistics))
                {
                    statisticsRoot.Add(new Label("Diagnostics unavailable / 진단 정보 없음"));
                    continue;
                }

                statisticsRoot.Add(new Label(
                    $"Active {statistics.CountActive} | Inactive {statistics.CountInactive} | " +
                    $"Created {statistics.CreatedCount} | Destroyed {statistics.DestroyedCount}"));
                statisticsRoot.Add(new Label(
                    $"Released {statistics.ReleasedCount} | Invalid {statistics.InvalidReleaseCount} | " +
                    $"Capacity Discarded {statistics.CapacityDiscardedCount}"));
            }
        }

        private static Label CreateBoldLabel(string text)
        {
            return new Label(text)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
        }

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
