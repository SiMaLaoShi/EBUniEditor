using UnityEditor;

namespace EBUniEditor.Editor.Inspector
{
    // [InitializeOnLoadAttribute]
    static class EditorHeaderExtension
    {
        static EditorHeaderExtension()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += DisplayGuidIfPersistent;
        }

        static void DisplayGuidIfPersistent(UnityEditor.Editor editor)
        {
            if (!EditorUtility.IsPersistent(editor.target))
                return;

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(editor.target));
            var totalRect = EditorGUILayout.GetControlRect();
            var controlRect = EditorGUI.PrefixLabel(totalRect, EditorGUIUtility.TrTempContent("GUID"));
            if (editor.targets.Length > 1)
                EditorGUI.LabelField(controlRect, EditorGUIUtility.TrTempContent("[Multiple objects selected]"));
            else
                EditorGUI.SelectableLabel(controlRect, guid);
        }
    }
}