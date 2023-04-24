using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Sprites;

namespace EBA.Ebunieditor.Editor.Shortcut
{
    /// <summary>
    /// 这个是用来快捷查看图集的窗口，说快捷也不快捷，其实就是把下拉框做成列表
    /// </summary>
    public class SpritePackerShortcutWindow : EditorWindow
    {
        [MenuItem("Window/SpritePackerShortcutWindow", false, 101)]
        static void ShowWindow()
        {
            var wnd = GetWindow<SpritePackerShortcutWindow>();
            wnd.maxSize = new Vector2(420, 650);
        }

        private string[] atlasNames;
        private Type packType;
        private EditorWindow packWind;
        private Vector2 scroll = Vector2.zero;

        private void OnEnable()
        {
            var assembly = Assembly.Load("UnityEditor");
            packType = assembly.GetType("UnityEditor.Sprites.PackerWindow");
            packWind = GetWindow(packType);
            atlasNames = Packer.atlasNames;
        }

        private void OnDisable()
        {
            packType = null;
            packWind = null;
            atlasNames = null;
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Sprite Packer Mode (AlwaysOnAtlas)", GUILayout.Width(300)))
            {
                EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOn;
                if (atlasNames.Length > 0)
                    SetSelectAtlasName(0);
            }

            if (GUILayout.Button("Pack", GUILayout.Width(100)))
            {
                Packer.RebuildAtlasCacheIfNeeded(EditorUserBuildSettings.activeBuildTarget, true);
                atlasNames = Packer.atlasNames;
                if (atlasNames.Length > 0)
                    SetSelectAtlasName(0);
            }

            GUILayout.EndHorizontal();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(600), GUILayout.Width(420));
            GUILayout.BeginVertical();
            for (var i = 0; i < atlasNames.Length; i++)
            {
                if (GUILayout.Button(atlasNames[i]))
                {
                    SetSelectAtlasName(i);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void SetSelectAtlasName(int selectIndex)
        {
            if (packWind == null)
            {
                packWind = GetWindow(packType);
            }

            // packType.SetFieldValue("m_SelectedAtlas", selectIndex);
            // packType.Invoke("RefreshAtlasPageList",  new object[]{});
            // packType.Invoke("Repaint", new object[]{});
            SetPackerWindowFieldValue("m_SelectedAtlas", selectIndex, BindingFlags.Instance | BindingFlags.NonPublic);
            CallPackerWindowMethod("RefreshAtlasPageList", null, BindingFlags.Instance | BindingFlags.NonPublic);
            CallPackerWindowMethod("Repaint", null, BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// 设置 Unity Sprite Packer(PackerWindow)内部的值，通过反射
        /// </summary>
        private void SetPackerWindowFieldValue(string fieldName, object value, BindingFlags bindFlags)
        {
            FieldInfo fieldInfo = packType.GetField(fieldName, bindFlags);
            if (fieldInfo != null) fieldInfo.SetValue(packWind, value);
        }

        /// <summary>
        /// 调用 Unity Sprite Packer(PackerWindow)的内部函数，通过反射
        /// </summary>
        private void CallPackerWindowMethod(string methodName, object[] parameters, BindingFlags bindFlags)
        {
            MethodInfo methodInfo = packType.GetMethod(methodName, bindFlags);
            if (methodInfo != null) methodInfo.Invoke(packWind, parameters);
        }
    }
}