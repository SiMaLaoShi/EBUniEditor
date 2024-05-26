using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Shortcut
{
    public class MenuShortcutsWindow : EditorWindow
    {
        private enum Page
        {
            Shortcuts,
            All
        }

        private class MenuItemData
        {
            private static readonly char[] separator = { '/', ' ' };
            public string menuItemPath = string.Empty;
            public string name = string.Empty;
            public string[] keywords;
            public string assemblyName = string.Empty;

            public void Init()
            {
                name = Path.GetFileNameWithoutExtension(menuItemPath);
                keywords = menuItemPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private static MenuShortcutsWindow s_window;
        private static SceneView.OnSceneFunc s_OnSceneGUI;
        private static EditorApplication.CallbackFunction s_OnEditorWindowUpdate;

        private static SortedDictionary<string, MenuItemData> s_allMenuItems =
            new SortedDictionary<string, MenuItemData>();

        private static readonly SortedDictionary<string, MenuItemData> s_shortcuts =
            new SortedDictionary<string, MenuItemData>();

        private static Vector2 s_scrollViewPos = Vector2.zero;
        private static Vector2 s_shortcutsViewPos = Vector2.zero;
        private static string s_search = string.Empty;
        private static List<string> s_assemblies = new List<string>();
        private static Page s_page = Page.Shortcuts;

        [MenuItem("Window/EBAWindow/ShortcutWindow", false, 100)]
        private static void Init()
        {
            if (s_window == null)
            {
                s_window = CreateInstance<MenuShortcutsWindow>();
                s_window.titleContent = new GUIContent("Shortcuts");
                CheckInitAllMenuItems();
            }

            if (s_shortcuts.Count == 0)
                s_page = Page.All;
            else
                s_page = Page.Shortcuts;

            s_window.Show();
            s_OnSceneGUI = OnSceneGUI;
            s_OnEditorWindowUpdate = OnEditorWindowUpdate;
            EditorApplication.update += s_OnEditorWindowUpdate;
            SceneView.onSceneGUIDelegate += s_OnSceneGUI;
        }

        private static void CheckInitAllMenuItems()
        {
            if (s_allMenuItems == null || s_allMenuItems.Count == 0)
            {
                var items = SearchAllMenuItems();
                s_allMenuItems = s_allMenuItems ?? new SortedDictionary<string, MenuItemData>();
                s_allMenuItems.Clear();
                var assemblies = new HashSet<string>();
                for (var i = 0; i < items.Count; ++i)
                {
                    var item = new MenuItemData();
                    var assemblyName = items[i].Value;
                    assemblies.Add(assemblyName);
                    s_allMenuItems.Add(items[i].Key, item);
                    item.menuItemPath = items[i].Key;
                    item.Init();
                }

                s_assemblies = assemblies.ToList();
                s_assemblies.Sort();
                Load();
            }
        }

        private static void Load()
        {
            s_search = EditorPrefs.GetString("MenuShortcutsWindow-search", string.Empty);
            s_shortcuts.Clear();
            var shortcuts = EditorPrefs.GetString("MenuShortcutsWindow-shortcuts", string.Empty).Split(';');
            if (shortcuts.Length > 0)
                for (var i = 0; i < shortcuts.Length; ++i)
                {
                    MenuItemData item;
                    if (s_allMenuItems.TryGetValue(shortcuts[i], out item) && item != null)
                        s_shortcuts.Add(shortcuts[i], item);
                }

            if (s_shortcuts.Count == 0)
                s_page = Page.All;
            else
                s_page = Page.Shortcuts;
        }

        private static void Save()
        {
            if (s_shortcuts != null && s_shortcuts.Count > 0)
            {
                var values = s_shortcuts.Keys.ToArray();
                Array.Sort(values);
                EditorPrefs.SetString("MenuShortcutsWindow-shortcuts", string.Join(";", values));
            }
            else
            {
                EditorPrefs.DeleteKey("MenuShortcutsWindow-shortcuts");
            }

            if (!string.IsNullOrEmpty(s_search))
                EditorPrefs.SetString("MenuShortcutsWindow-search", s_search);
            else
                EditorPrefs.DeleteKey("MenuShortcutsWindow-search");
        }

        private static List<KeyValuePair<string, string>> SearchAllMenuItems()
        {
            var ret = new List<KeyValuePair<string, string>>();
            var set = new HashSet<string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; ++i)
            {
                var types = assemblies[i].GetTypes();
                var assemblyName = assemblies[i].GetName().Name;
                for (var j = 0; j < types.Length; ++j)
                {
                    var methods = types[j]
                        .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    for (var n = 0; n < methods.Length; ++n)
                    {
                        var attrs = methods[n].GetCustomAttributes(typeof(MenuItem), false);
                        if (attrs.Length > 0)
                            for (var m = 0; m < attrs.Length; ++m)
                            {
                                var attr = attrs[m] as MenuItem;
                                if (!string.IsNullOrEmpty(attr.menuItem))
                                    if (set.Add(attr.menuItem))
                                        ret.Add(new KeyValuePair<string, string>(attr.menuItem, assemblyName));
                            }
                    }
                }
            }

            return ret;
        }

        private void OnDestroy()
        {
            Save();
            s_window = null;
            SceneView.onSceneGUIDelegate -= s_OnSceneGUI;
            EditorApplication.update -= s_OnEditorWindowUpdate;
            s_OnSceneGUI = null;
            s_OnEditorWindowUpdate = null;
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        private static void OnEditorWindowUpdate()
        {
        }

        private static void OnSceneGUI(SceneView sceneview)
        {
        }

        private static void Clear()
        {
            s_search = string.Empty;
            s_shortcuts.Clear();
            Save();
        }

        private static void OnGUI_Shortcuts()
        {
            s_shortcutsViewPos = EditorGUILayout.BeginScrollView(s_shortcutsViewPos);
            List<string> removeKeys = null;
            foreach (var kv in s_shortcuts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(kv.Value.name);
                GUI.color = Color.red;
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    removeKeys = removeKeys ?? new List<string>();
                    removeKeys.Add(kv.Key);
                }

                GUI.color = Color.green;
                if (GUILayout.Button("Excute", GUILayout.Width(60))) EditorApplication.ExecuteMenuItem(kv.Key);

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            if (removeKeys != null)
            {
                for (var i = 0; i < removeKeys.Count; ++i) s_shortcuts.Remove(removeKeys[i]);

                Save();
            }

            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Clear")) Clear();
        }

        private static void OnGUI_ShowAll()
        {
            s_scrollViewPos = EditorGUILayout.BeginScrollView(s_scrollViewPos);
            var changed = false;
            foreach (var kv in s_allMenuItems)
            {
                if (!string.IsNullOrEmpty(s_search) &&
                    kv.Key.IndexOf(s_search, StringComparison.CurrentCultureIgnoreCase) == -1)
                    continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(kv.Key);
                if (s_shortcuts.ContainsKey(kv.Key))
                {
                    GUI.color = Color.red;
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        s_shortcuts.Remove(kv.Key);
                        changed = true;
                    }
                }
                else
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        s_shortcuts.Add(kv.Key, kv.Value);
                        changed = true;
                    }
                }

                GUI.color = Color.white;
                if (GUILayout.Button("Excute", GUILayout.Width(60))) EditorApplication.ExecuteMenuItem(kv.Key);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            if (changed) Save();
        }

        private void OnGUI()
        {
            CheckInitAllMenuItems();
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 32;
            s_search = EditorGUILayout.TextField("Find", s_search);
            if (s_search.StartsWith(" "))
            {
                var search = s_search.TrimStart(' ');
                if (string.IsNullOrEmpty(search)) s_search = string.Empty;
            }

            s_page = (Page)GUILayout.Toolbar((int)s_page, Enum.GetNames(typeof(Page)));
            switch (s_page)
            {
                case Page.Shortcuts:
                    OnGUI_Shortcuts();
                    break;
                case Page.All:
                    OnGUI_ShowAll();
                    break;
            }

            EditorGUILayout.EndVertical();
        }
    }
}