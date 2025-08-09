#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EBUniEditor.Editor.Inspector {

    public static class EBInspectorToolkit {

        private static List<EBInspectorContainer> containers = new List<EBInspectorContainer>();
        private static GUIStyle boldLabelStyle;
        private static EBInspectorPersistentData persistentData;
        
        private static Type inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        private static FieldInfo allInspectorsFieldInfo = inspectorWindowType.GetField("m_AllInspectors", BindingFlags.NonPublic | BindingFlags.Static);

        private const string PersistentDataName = "EBInspectorPersistentData";
        
        private static bool isToolkitDisabled;
        private const string DisableMenuPath = "Tools/EBInspector Toolkit/Disable Toolkit";
        private const string SettingsMenuPath = "Tools/EBInspector Toolkit/Settings";
        private const string ToolkitEnabledPrefKey = "EBInspectorToolkit_EnableState";
        
        [InitializeOnLoadMethod] 
        private static void Initialize() {
            isToolkitDisabled = EditorPrefs.GetBool(ToolkitEnabledPrefKey, false);
            if (isToolkitDisabled) return;
            
            EditorApplication.delayCall += InitializeToolkit;
        }

        [MenuItem(DisableMenuPath)]
        private static void ToggleToolkit() {
            isToolkitDisabled = !isToolkitDisabled;
            EditorPrefs.SetBool(ToolkitEnabledPrefKey, isToolkitDisabled);

            if (isToolkitDisabled) {
                ShutdownToolkit();
            }
            else {
                InitializeToolkit();
            }
        }
        
        [MenuItem(DisableMenuPath, true)]
        private static bool ValidateToggleToolkit() {
            Menu.SetChecked(DisableMenuPath, isToolkitDisabled);
            return true;
        }

        [MenuItem(SettingsMenuPath)]
        private static void OpenSettings() {
            EBInspectorSettings.ShowSettingsWindow(); 
        }

        private static void InitializeToolkit() {
            try {
                boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 10
                };

                InitializePersistentData();
                SubscribeToEvents();
                
                float searchFieldHeight = EditorStyles.toolbarSearchField.fixedHeight;
                EBInspectorContainer.BoldLabelStyle = boldLabelStyle;
                EBInspectorContainer.SearchBarHeight = searchFieldHeight;
                
                EBInspectorContainer.RightToolBarStyle ??= new GUIStyle(EditorStyles.miniButtonRight) { fixedHeight = searchFieldHeight };
                EBInspectorContainer.LeftToolBarStyle  ??= new GUIStyle(EditorStyles.miniButtonLeft)  { fixedHeight = searchFieldHeight };
                
                EBInspectorContainer.CopyButtonContent  ??= new GUIContent(string.Empty, "Copy selected components to clipboard");
                EBInspectorContainer.PasteButtonContent ??= new GUIContent(string.Empty, "Paste clipboard components");

                EBInspectorContainer.IconAtlas ??= AssetDatabase.LoadAssetAtPath<Texture>($"{GetToolkitAssetPath()}/EBInspectorIcons.png");
                EBInspectorContainer.CloseIcon ??= EditorGUIUtility.IconContent("CrossIcon").image;
                
                EBInspectorContainer.AllComponentsIcon = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_GridLayoutGroup Icon" : "GridLayoutGroup Icon").image;
                EBInspectorSettings.LoadSettings();
            }
            catch {
                EditorApplication.delayCall += InitializeToolkit;
            }
        }

        private static void ShutdownToolkit() {
            UnsubscribeFromEvents();
            foreach (EBInspectorContainer container in containers) {
                container.RemoveGUI();
            }
            containers.Clear();
        }

        private static void SubscribeToEvents() {
            EditorApplication.update -= RefreshInspectorWindows;
            EditorApplication.update += RefreshInspectorWindows;
            
            EditorApplication.update -= UpdateContainers;
            EditorApplication.update += UpdateContainers;
            
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
            
            EditorApplication.quitting -= OnApplicationQuit;
            EditorApplication.quitting += OnApplicationQuit;

            EBInspectorSettings.OnSettingsChanged += OnSettingsChanged;
        }

        private static void UnsubscribeFromEvents() {
            EditorApplication.update -= RefreshInspectorWindows;
            EditorApplication.update -= UpdateContainers;
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.quitting -= OnApplicationQuit;
            EBInspectorSettings.OnSettingsChanged -= OnSettingsChanged;
        }
        
        private static void RefreshInspectorWindows() {
            IList inspectorWindows = (IList)allInspectorsFieldInfo.GetValue(inspectorWindowType);
            
            if (inspectorWindows == null || inspectorWindows.Count <= 0) {
                containers.Clear();
                return;
            }
            
            foreach (EditorWindow inspectorWindow in inspectorWindows) {
                if (!HasContainerForInspector(inspectorWindow)) {
                    containers.Add(new EBInspectorContainer(inspectorWindow, Selection.activeObject));
                }
            }
            
            for (int i = containers.Count - 1; i >= 0; i--) {
                if (!containers[i].InspectorWindow) {
                    containers.RemoveAt(i);
                }
            }
        }

        private static bool HasContainerForInspector(EditorWindow inspector) {
            foreach (EBInspectorContainer container in containers) {
                if (container.InspectorWindow.GetInstanceID() == inspector.GetInstanceID()) {
                    return true;
                }
            }
            return false;
        }
        
        private static void OnSelectionChanged() {
            foreach (EBInspectorContainer container in containers) {
                if (!container.IsInspectorLocked()) {
                    container.SetTargetObject(Selection.activeObject);
                }
                container.UpdateContainer();
            }
        }

        private static void UpdateContainers() {
            InitializePersistentData();
            foreach (EBInspectorContainer container in containers) {
                container.UpdateContainer();
            }
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect) {
            foreach (EBInspectorContainer container in containers) {
                container.OnHierarchyGUI();
            }
        }

        private static void InitializePersistentData() {
            if (persistentData) return;
            
            string assetPath = GetToolkitAssetPath();
            string persistentPath = $"{assetPath}/{PersistentDataName}.asset";

            persistentData = AssetDatabase.LoadAssetAtPath<EBInspectorPersistentData>(persistentPath);
            
            if (!persistentData) {
                persistentData = ScriptableObject.CreateInstance<EBInspectorPersistentData>();
                persistentData.name = PersistentDataName;
                AssetDatabase.CreateAsset(persistentData, persistentPath);
                AssetDatabase.SaveAssets();
            }
            
            EBInspectorContainer.PersistentData = persistentData;
        }
        
        private static string GetToolkitAssetPath() {
            string[] assetIds = AssetDatabase.FindAssets($"{nameof(EBInspectorToolkit)}");
            foreach (string assetId in assetIds) {
                string filePath = AssetDatabase.GUIDToAssetPath(assetId);
                string fileName = Path.GetFileName(filePath);
                if (fileName == $"{nameof(EBInspectorToolkit)}.cs") {
                    return Path.GetDirectoryName(filePath);
                }
            }
            return string.Empty;
        }

        private static void OnApplicationQuit() {
            persistentData?.ClearAllData();
        }

        private static void OnSettingsChanged() {
            foreach (EBInspectorContainer container in containers) {
                container.RemoveGUI();
                container.UpdateContainer();
                container.InspectorWindow.Repaint();
            }
        }
    }
}
#endif

