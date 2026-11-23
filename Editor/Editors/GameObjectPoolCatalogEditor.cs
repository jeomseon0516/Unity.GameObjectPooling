using System.Collections.Generic;
using Jeomseon.Unity.GameObjectPooling.Definitions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.GameObjectPooling.Editor.Editors
{
    [CustomEditor(typeof(GameObjectPoolCatalog))]
    internal sealed class GameObjectPoolCatalogEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var warningsRoot = new VisualElement();
            root.Add(warningsRoot);
            RefreshWarnings(warningsRoot);
            root.TrackSerializedObjectValue(serializedObject, _ => RefreshWarnings(warningsRoot));
            return root;
        }

        private void RefreshWarnings(VisualElement warningsRoot)
        {
            warningsRoot.Clear();
            if (target == null) return;

            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty definitions = serializedObject.FindProperty("definitions");
            var uniqueDefinitions = new HashSet<Object>();
            for (int i = 0; i < definitions.arraySize; i++)
            {
                Object definition = definitions.GetArrayElementAtIndex(i).objectReferenceValue;
                if (definition == null)
                {
                    warningsRoot.Add(new HelpBox(
                        $"Element {i} is null. / {i}번 항목이 비어 있습니다.",
                        HelpBoxMessageType.Warning));
                }
                else if (!uniqueDefinitions.Add(definition))
                {
                    warningsRoot.Add(new HelpBox(
                        $"{definition.name} is duplicated. / {definition.name}이 중복됐습니다.",
                        HelpBoxMessageType.Warning));
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            SerializedProperty definitions = serializedObject.FindProperty("definitions");
            var uniqueDefinitions = new HashSet<Object>();
            for (int i = 0; i < definitions.arraySize; i++)
            {
                Object definition = definitions.GetArrayElementAtIndex(i).objectReferenceValue;
                if (definition == null)
                {
                    EditorGUILayout.HelpBox(
                        $"Element {i} is null. / {i}번 항목이 비어 있습니다.",
                        MessageType.Warning);
                }
                else if (!uniqueDefinitions.Add(definition))
                {
                    EditorGUILayout.HelpBox(
                        $"{definition.name} is duplicated. / {definition.name}이 중복됐습니다.",
                        MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
