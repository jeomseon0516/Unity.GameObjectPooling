using Jeomseon.Unity.GameObjectPooling.Definitions;
using Jeomseon.Unity.GameObjectPooling.Configurations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.GameObjectPooling.Editor.Editors
{
    [CustomEditor(typeof(UnityGameObjectPoolDefinition))]
    internal sealed class UnityGameObjectPoolDefinitionEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var prefabError = new HelpBox(
                "A prefab is required. / Prefab을 지정해야 합니다.",
                HelpBoxMessageType.Error);
            var prewarmError = new HelpBox(
                "Prewarm Count cannot exceed Max Inactive Count. / " +
                "Prewarm Count는 Max Inactive Count보다 클 수 없습니다.",
                HelpBoxMessageType.Error);
            var preserveWarning = new HelpBox(
                "Active instances remain alive after this pool is destroyed. Their owner " +
                "must stop and destroy them explicitly; they can no longer return to this " +
                "pool. / 이 Pool이 파괴되어도 사용 중인 오브젝트는 남습니다. 더 이상 " +
                "Pool로 반환할 수 없으므로 소유자가 직접 종료하고 파괴해야 합니다.",
                HelpBoxMessageType.Warning);
            root.Add(prefabError);
            root.Add(prewarmError);
            root.Add(preserveWarning);

            void Refresh()
            {
                if (target == null) return;

                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty prefab = serializedObject.FindProperty("prefab");
                SerializedProperty prewarm = serializedObject.FindProperty("prewarmCount");
                SerializedProperty maxInactive = serializedObject.FindProperty("maxInactiveCount");
                SerializedProperty shutdownPolicy =
                    serializedObject.FindProperty("activeInstanceShutdownPolicy");
                SetVisible(prefabError, prefab.objectReferenceValue == null);
                SetVisible(prewarmError, prewarm.intValue > maxInactive.intValue);
                SetVisible(
                    preserveWarning,
                    (ActiveInstanceShutdownPolicy)shutdownPolicy.enumValueIndex ==
                    ActiveInstanceShutdownPolicy.Preserve);
            }

            Refresh();
            root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            return root;
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            SerializedProperty prefab = serializedObject.FindProperty("prefab");
            SerializedProperty prewarm = serializedObject.FindProperty("prewarmCount");
            SerializedProperty maxInactive = serializedObject.FindProperty("maxInactiveCount");
            SerializedProperty shutdownPolicy =
                serializedObject.FindProperty("activeInstanceShutdownPolicy");
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

            if ((ActiveInstanceShutdownPolicy)shutdownPolicy.enumValueIndex ==
                ActiveInstanceShutdownPolicy.Preserve)
            {
                EditorGUILayout.HelpBox(
                    "Active instances remain alive after this pool is destroyed. Their owner " +
                    "must stop and destroy them explicitly; they can no longer return to this " +
                    "pool. / 이 Pool이 파괴되어도 사용 중인 오브젝트는 남습니다. 더 이상 " +
                    "Pool로 반환할 수 없으므로 소유자가 직접 종료하고 파괴해야 합니다.",
                    MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
