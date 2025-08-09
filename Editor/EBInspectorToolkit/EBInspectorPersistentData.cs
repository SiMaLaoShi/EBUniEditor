#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EBUniEditor.Editor.Inspector {

    public class EBInspectorPersistentData : ScriptableObject {

        public readonly EBComponentClipboard Clipboard = new EBComponentClipboard();

        [SerializeField] private List<int> indexLookUp = new List<int>();
        [SerializeField] private List<string> searchFields = new List<string>();
        [SerializeField] private List<ComponentSelectionData> selectedCompIds = new List<ComponentSelectionData>();
        
        [Serializable]
        private class ComponentSelectionData {
            public List<int> selectionList = new List<int>();
        }

        public List<int> GetSelectedComponentIds(Object obj) {
            if (GetObjectIndex(obj, out int index)) {
                return selectedCompIds[index].selectionList;
            }
            return null;
        } 
        
        public string GetSearchString(Object obj) {
            if (GetObjectIndex(obj, out int index)) {
                return searchFields[index];
            }
            return string.Empty;
        }

        public void SetSearchString(Object obj, string str) {
            if (GetObjectIndex(obj, out int index)) {
                searchFields[index] = str;
            }
        }

        public void AddDataForObject(Object obj) {
            int id = obj.GetInstanceID();

            int index = indexLookUp.BinarySearch(id);
            if (index >= 0) return;
            
            index = ~index; 
            indexLookUp.Insert(index, id); 
            selectedCompIds.Insert(index, new ComponentSelectionData());
            searchFields.Insert(index, string.Empty);
        }

        public void ClearAllData() {
            indexLookUp.Clear();
            selectedCompIds.Clear();
            searchFields.Clear();
            AssetDatabase.SaveAssetIfDirty(this);
        }
        
        private bool GetObjectIndex(Object obj, out int index) {
            index = indexLookUp.BinarySearch(obj.GetInstanceID());
            return index >= 0;
        }
        
        [CustomEditor(typeof(EBInspectorPersistentData))]
        private class EBInspectorDataEditor : UnityEditor.Editor {
            
            public override void OnInspectorGUI() {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true
                };

                EditorGUILayout.LabelField(
                    $"Stores persistent data for {nameof(EBInspectorToolkit)} like selected components and search strings.\n\n" +
                    "This data clears every time the editor is restarted.\n\n" +
                    "This file can be safely ignored by version control.", 
                    labelStyle 
                );
            }
        }
    }
}
#endif

