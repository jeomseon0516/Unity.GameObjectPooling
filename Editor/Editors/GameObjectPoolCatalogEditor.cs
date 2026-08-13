using System.Collections.Generic;
using Jeomseon.Unity.GameObjectPooling.Definitions;
using UnityEditor;
using UnityEngine;

namespace Jeomseon.Unity.GameObjectPooling.Editor.Editors
{
    [CustomEditor(typeof(GameObjectPoolCatalog))]
    internal sealed class GameObjectPoolCatalogEditor : UnityEditor.Editor
    {
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
