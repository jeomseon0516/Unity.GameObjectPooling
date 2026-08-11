using Jeomseon.GameObjectPooling.Definitions;
using UnityEditor;

namespace Jeomseon.GameObjectPooling.Editors
{
    [CustomEditor(typeof(UnityGameObjectPoolDefinition))]
    internal sealed class UnityGameObjectPoolDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            SerializedProperty prefab = serializedObject.FindProperty("prefab");
            SerializedProperty prewarm = serializedObject.FindProperty("prewarmCount");
            SerializedProperty maxInactive = serializedObject.FindProperty("maxInactiveCount");
            if (prefab.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "A prefab is required. / Prefab을 지정해야 합니다.",
                    MessageType.Error);
            }

            if (prewarm.intValue > maxInactive.intValue)
            {
                EditorGUILayout.HelpBox(
                    "Prewarm Count cannot exceed Max Inactive Count. / " +
                    "Prewarm Count는 Max Inactive Count보다 클 수 없습니다.",
                    MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
