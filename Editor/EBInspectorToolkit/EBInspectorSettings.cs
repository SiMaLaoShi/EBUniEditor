#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EBUniEditor.Editor.Inspector {

    public class EBInspectorSettings : EditorWindow {

        public static Action OnSettingsChanged;
        
        public static bool HideToolbar { get; private set; }
        public static bool HideCopyPaste { get; private set; }
        public static bool TransformOnlyDisable { get; private set; }
        public static bool TransformOnlyKeepCopyPaste { get; private set; }
        public static int MaxNumberOfRows { get; private set; } = 3;

        private const string HideToolbarKey = "EBInspectorHideToolBar"; 
        private const string HideCopyPasteKey = "EBInspectorHideCopyPaste"; 
        private const string TransOnlyDisableKey = "EBInspectorTransformDisable"; 
        private const string TransOnlyKeepCopyPasteKey = "EBInspectorTransformKeepCopyPaste"; 
        private const string MaxNumberOfRowsKey = "EBInspectorNumberOfRows";

        private const string MaxRowsSettingName = "Max number of component rows"; 
        private const string HideToolbarSettingName = "Hide toolbar"; 
        private const string HideCpSettingName = "Hide copy & paste buttons"; 
        private const string HideInspectorSettingName = "Hide Inspector Toolkit entirely";
        private const string OnlyCpSettingName = "Only show copy & paste buttons";

        public static void LoadSettings() {
            HideToolbar = EditorPrefs.GetBool(HideToolbarKey, false);
            HideCopyPaste = EditorPrefs.GetBool(HideCopyPasteKey, false);
            TransformOnlyDisable = EditorPrefs.GetBool(TransOnlyDisableKey, false);
            TransformOnlyKeepCopyPaste = EditorPrefs.GetBool(TransOnlyKeepCopyPasteKey, false);
            MaxNumberOfRows = EditorPrefs.GetInt(MaxNumberOfRowsKey, 3);
        }
        
        public static void ShowSettingsWindow() {
            EBInspectorSettings window = GetWindow<EBInspectorSettings>("EBInspector Settings");
            window.ShowUtility();
        }

        private void OnGUI() {
            EditorGUI.BeginChangeCheck();
            
            GUILayout.Label("Display", EditorStyles.largeLabel);
            {
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 200;
                MaxNumberOfRows = EditorGUILayout.IntSlider(MaxRowsSettingName, MaxNumberOfRows, 1, 10);
                EditorGUIUtility.labelWidth = previousLabelWidth;
                
                HideToolbar = GUILayout.Toggle(HideToolbar, HideToolbarSettingName);
                
                EditorGUI.BeginDisabledGroup(HideToolbar);
                HideCopyPaste = GUILayout.Toggle(HideToolbar ? false : HideCopyPaste, HideCpSettingName);
                EditorGUI.EndDisabledGroup();
                
                if (HideToolbar || HideCopyPaste) {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("Copy & paste is still available via the context menu", MessageType.Info);
                }
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("When Transform Only", EditorStyles.largeLabel);
            {
                TransformOnlyDisable = GUILayout.Toggle(TransformOnlyDisable, HideInspectorSettingName);
                
                EditorGUI.BeginDisabledGroup(TransformOnlyDisable);
                TransformOnlyKeepCopyPaste = GUILayout.Toggle(TransformOnlyDisable ? false : TransformOnlyKeepCopyPaste, OnlyCpSettingName);
                EditorGUI.EndDisabledGroup();
                
                if (TransformOnlyKeepCopyPaste) {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox($"'{OnlyCpSettingName}' overrides '{HideCpSettingName}' when active.", MessageType.Info);
                }
            }
            
            if (EditorGUI.EndChangeCheck()) {
                SaveSettings();
                OnSettingsChanged?.Invoke();
            }
        }

        private static void SaveSettings() {
            EditorPrefs.SetBool(HideToolbarKey, HideToolbar);
            EditorPrefs.SetBool(HideCopyPasteKey, HideCopyPaste);
            EditorPrefs.SetBool(TransOnlyDisableKey, TransformOnlyDisable);
            EditorPrefs.SetBool(TransOnlyKeepCopyPasteKey, TransformOnlyKeepCopyPaste);
            EditorPrefs.SetInt(MaxNumberOfRowsKey, MaxNumberOfRows);
        }
    }
}
#endif

