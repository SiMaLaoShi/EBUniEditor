#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EBUniEditor.Editor.Inspector {

    public class UnityIconViewer : EditorWindow {

        private static List<IconData> allIcons = new List<IconData>();
        private static List<IconData> filteredIcons = new List<IconData>();
        private Vector2 scrollPosition;
        private string searchText = "";
        private int iconsPerRow = 6;
        private float iconSize = 64f;
        private bool showIconName = true;
        private bool showTooltip = true;
        private bool darkThemeOnly = false;
        private bool lightThemeOnly = false;

        [Serializable]
        private class IconData {
            public string name;
            public Texture2D texture;
            public GUIContent content;
            public bool isDarkTheme;
        }

        [MenuItem("Tools/Unity Icon Viewer")]
        public static void ShowWindow() {
            UnityIconViewer window = GetWindow<UnityIconViewer>("Unity Icon Viewer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable() {
            if (allIcons.Count == 0) {
                LoadAllIcons();
            }
            FilterIcons();
        }

        private void OnGUI() {
            DrawToolbar();
            DrawIconGrid();
        }

        private void DrawToolbar() {
            EditorGUILayout.BeginVertical(EditorStyles.toolbar);
            
            // 搜索框
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            
            EditorGUI.BeginChangeCheck();
            searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck()) {
                FilterIcons();
            }
            
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                searchText = "";
                FilterIcons();
            }
            EditorGUILayout.EndHorizontal();
            
            // 设置选项
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            iconsPerRow = EditorGUILayout.IntSlider("Icons Per Row", iconsPerRow, 2, 10);
            iconSize = EditorGUILayout.Slider("Icon Size", iconSize, 16f, 128f);
            if (EditorGUI.EndChangeCheck()) {
                Repaint();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 显示选项
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            showIconName = EditorGUILayout.Toggle("Show Names", showIconName);
            showTooltip = EditorGUILayout.Toggle("Show Tooltip", showTooltip);
            
            bool newDarkOnly = EditorGUILayout.Toggle("Dark Only", darkThemeOnly);
            bool newLightOnly = EditorGUILayout.Toggle("Light Only", lightThemeOnly);
            
            if (newDarkOnly != darkThemeOnly || newLightOnly != lightThemeOnly) {
                darkThemeOnly = newDarkOnly;
                lightThemeOnly = newLightOnly;
                
                // 互斥选择
                if (darkThemeOnly && lightThemeOnly) {
                    lightThemeOnly = false;
                }
                
                FilterIcons();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 刷新按钮和统计信息
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Icons", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                LoadAllIcons();
                FilterIcons();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Total: {allIcons.Count} | Filtered: {filteredIcons.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void DrawIconGrid() {
            if (filteredIcons.Count == 0) {
                EditorGUILayout.HelpBox("No icons found. Try adjusting your search or filter settings.", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            float windowWidth = position.width - 30; // 减去滚动条宽度
            float cellWidth = windowWidth / iconsPerRow;
            float cellHeight = iconSize + (showIconName ? 40 : 20);
            
            int columns = iconsPerRow;
            int rows = Mathf.CeilToInt((float)filteredIcons.Count / columns);
            
            for (int row = 0; row < rows; row++) {
                EditorGUILayout.BeginHorizontal();
                
                for (int col = 0; col < columns; col++) {
                    int index = row * columns + col;
                    if (index >= filteredIcons.Count) {
                        GUILayout.FlexibleSpace();
                        continue;
                    }
                    
                    DrawIconCell(filteredIcons[index], cellWidth, cellHeight);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawIconCell(IconData iconData, float cellWidth, float cellHeight) {
            EditorGUILayout.BeginVertical(GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
            
            // 绘制图标
            Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.ExpandWidth(true));
            iconRect.size = new Vector2(iconSize, iconSize);
            iconRect.x += (cellWidth - iconSize) * 0.5f; // 居中
            
            if (iconData.texture != null) {
                GUI.DrawTexture(iconRect, iconData.texture, ScaleMode.ScaleToFit);
                
                // 点击复制名称
                if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition)) {
                    EditorGUIUtility.systemCopyBuffer = iconData.name;
                    Debug.Log($"Copied icon name to clipboard: {iconData.name}");
                    Event.current.Use();
                }
                
                // 鼠标悬停显示tooltip
                if (showTooltip && iconRect.Contains(Event.current.mousePosition)) {
                    string tooltip = $"Name: {iconData.name}\nClick to copy name\nSize: {iconData.texture.width}x{iconData.texture.height}";
                    if (iconData.isDarkTheme) {
                        tooltip += "\nTheme: Dark";
                    }
                    
                    Vector2 tooltipSize = GUI.skin.box.CalcSize(new GUIContent(tooltip));
                    Vector2 mousePos = Event.current.mousePosition;
                    Rect tooltipRect = new Rect(mousePos.x + 10, mousePos.y - tooltipSize.y - 10, tooltipSize.x + 10, tooltipSize.y + 5);
                    
                    // 确保tooltip在窗口内
                    if (tooltipRect.xMax > position.width) {
                        tooltipRect.x = mousePos.x - tooltipRect.width - 10;
                    }
                    if (tooltipRect.y < 0) {
                        tooltipRect.y = mousePos.y + 20;
                    }
                    
                    GUI.Box(tooltipRect, tooltip);
                    Repaint();
                }
            }
            else {
                GUI.Box(iconRect, "Missing");
            }
            
            // 显示图标名称
            if (showIconName) {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.wordWrap = true;
                
                string displayName = iconData.name;
                if (displayName.Length > 15) {
                    displayName = displayName.Substring(0, 12) + "...";
                }
                
                EditorGUILayout.LabelField(displayName, labelStyle, GUILayout.Height(30));
            }
            
            EditorGUILayout.EndVertical();
        }

        private void LoadAllIcons() {
            allIcons.Clear();
            
            // 使用反射获取所有内置图标
            HashSet<string> iconNames = new HashSet<string>();
            
            // 方法1: 通过EditorGUIUtility的内部字段获取
            try {
                Type editorGUIUtilityType = typeof(EditorGUIUtility);
                FieldInfo iconsField = editorGUIUtilityType.GetField("s_IconGUIContents", BindingFlags.NonPublic | BindingFlags.Static);
                
                if (iconsField != null) {
                    var iconsDict = iconsField.GetValue(null) as System.Collections.IDictionary;
                    if (iconsDict != null) {
                        foreach (var key in iconsDict.Keys) {
                            iconNames.Add(key.ToString());
                        }
                    }
                }
            }
            catch (Exception) {
                // 忽略反射错误
            }
            
            // 方法2: 添加常用的已知图标名称
            string[] commonIcons = {
                // 基础图标
                "CrossIcon", "CheckIcon", "AlertIcon", "InfoIcon", "WarningIcon", "ErrorIcon",
                "AddIcon", "RemoveIcon", "RefreshIcon", "SearchIcon", "SettingsIcon", "HelpIcon",
                
                // 组件图标
                "Transform Icon", "Camera Icon", "Light Icon", "AudioSource Icon", "Rigidbody Icon",
                "BoxCollider Icon", "SphereCollider Icon", "CapsuleCollider Icon", "MeshCollider Icon",
                "Animator Icon", "Animation Icon", "ParticleSystem Icon", "LineRenderer Icon",
                "MeshRenderer Icon", "SkinnedMeshRenderer Icon", "SpriteRenderer Icon",
                "Canvas Icon", "EventSystem Icon", "GraphicRaycaster Icon",
                
                // UI图标
                "RectTransform Icon", "Image Icon", "Text Icon", "Button Icon", "Toggle Icon",
                "Slider Icon", "Scrollbar Icon", "Dropdown Icon", "InputField Icon",
                "ScrollRect Icon", "Mask Icon", "CanvasGroup Icon",
                
                // 编辑器图标
                "SceneAsset Icon", "PrefabAsset Icon", "Material Icon", "Texture2D Icon", "AudioClip Icon",
                "AnimationClip Icon", "RuntimeAnimatorController Icon", "Avatar Icon", "Mesh Icon",
                "Shader Icon", "ComputeShader Icon", "Script Icon", "Folder Icon",
                
                // 工具图标
                "MoveTool", "RotateTool", "ScaleTool", "RectTool", "TransformTool",
                "ViewTool", "HandTool", "PlayButton", "PauseButton", "StepButton",
                "RecordButton", "RecordOn", "RecordOff",
                
                // 窗口图标
                "UnityEditor.HierarchyWindow", "UnityEditor.ProjectBrowser", "UnityEditor.InspectorWindow",
                "UnityEditor.SceneView", "UnityEditor.GameView", "UnityEditor.ConsoleWindow",
                "UnityEditor.ProfilerWindow", "UnityEditor.AnimationWindow",
                
                // 其他常用图标
                "TreeEditor.Duplicate", "TreeEditor.Trash", "Clipboard", "Import", "Export",
                "GridLayoutGroup Icon", "HorizontalLayoutGroup Icon", "VerticalLayoutGroup Icon",
                "ContentSizeFitter Icon", "AspectRatioFitter Icon", "LayoutElement Icon",
                
                // 暗色主题图标 (d_ 前缀)
                "d_CrossIcon", "d_CheckIcon", "d_AlertIcon", "d_InfoIcon", "d_WarningIcon", "d_ErrorIcon",
                "d_AddIcon", "d_RemoveIcon", "d_RefreshIcon", "d_SearchIcon", "d_SettingsIcon", "d_HelpIcon",
                "d_Transform Icon", "d_Camera Icon", "d_Light Icon", "d_GridLayoutGroup Icon",
                "d_TreeEditor.Duplicate", "d_TreeEditor.Trash", "d_Clipboard", "d_Import", "d_Export"
            };
            
            foreach (string iconName in commonIcons) {
                iconNames.Add(iconName);
            }
            
            // 创建IconData列表
            foreach (string iconName in iconNames) {
                GUIContent content = EditorGUIUtility.IconContent(iconName);
                if (content != null && content.image != null) {
                    IconData iconData = new IconData {
                        name = iconName,
                        texture = content.image as Texture2D,
                        content = content,
                        isDarkTheme = iconName.StartsWith("d_")
                    };
                    allIcons.Add(iconData);
                }
            }
            
            // 按名称排序
            allIcons.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            
            Debug.Log($"Loaded {allIcons.Count} Unity icons");
        }

        private void FilterIcons() {
            filteredIcons.Clear();
            
            foreach (IconData icon in allIcons) {
                // 搜索过滤
                if (!string.IsNullOrEmpty(searchText) && 
                    !icon.name.ToLower().Contains(searchText.ToLower())) {
                    continue;
                }
                
                // 主题过滤
                if (darkThemeOnly && !icon.isDarkTheme) {
                    continue;
                }
                
                if (lightThemeOnly && icon.isDarkTheme) {
                    continue;
                }
                
                filteredIcons.Add(icon);
            }
        }
    }
}

#endif

