using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EBA.Ebunieditor.Editor.Common;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[InitializeOnLoad]
public static class DefaultHeaderDrawer
{
    
    static DefaultHeaderDrawer()
    {
        Editor.finishedDefaultHeaderGUI += OnDefaultHeaderGUI;
    }

    private static void OnDefaultHeaderGUI(Editor editor)
    {
        if (GlobalScriptableObject.instance.isShowQuickComponent)
        {
            GameObject obj = editor.target as GameObject;
            if (obj)
            {
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.DropdownButton(new GUIContent("复制组件"), FocusType.Passive, "DropDownButton"))
                {
                    ShowComponentList(obj);
                }
                
                if (EditorGUILayout.DropdownButton(new GUIContent("粘贴组件"), FocusType.Passive, "DropDownButton"))
                {
                    ShowPasteComponentList(obj);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private static List<MenuComponent> menuComponents = new List<MenuComponent>();
    private static List<Component> srcComponents = new List<Component>();
    private static Dictionary<Type, Component> srcComponentsDic = new Dictionary<Type, Component>();
    private const string k_All = "All";
    private const string k_None = "None";
    private static void ShowComponentList(GameObject obj)
    {
        Component[] components = obj.GetComponents(typeof(Component));
        GenericMenu menu = new GenericMenu();

        foreach (var component in components)
        {
            menu.AddItem(new GUIContent(component.GetType().Name), srcComponentsDic.ContainsValue(component), OnComponentSelected, component);
        }
        menu.ShowAsContext();
    }
    
    private static bool AreComponentValuesEqual(Component a, Component b)
    {
        if (a.GetType() != b.GetType())
            return false;
        return a == b;
    }
    
    private static void ShowPasteComponentList(GameObject obj)
    {
        GenericMenu menu = new GenericMenu();
        // 获取obj上所有组件的类型列表
        var existingComponents = obj.GetComponents<Component>().ToList();

        foreach (var kv in srcComponentsDic)
        {
            // 检查obj是否已有具有相同值的组件实例
            var existingComponent = existingComponents.FirstOrDefault(c => AreComponentValuesEqual(c, kv.Value));
            var on = existingComponent != null;

            menu.AddItem(new GUIContent(kv.Key.Name), on, (data =>
            {
                var comType = data as Type;
                if (comType == null)
                    return;

                var srcComponent = kv.Value;

                if (!existingComponents.Any(c => c.GetType() == comType))
                {
                    // 如果没有该类型的组件，则粘贴一个新的组件到obj上
                    ComponentUtility.CopyComponent(srcComponent);
                    ComponentUtility.PasteComponentAsNew(obj);
                }
                else
                {
                    // 如果obj已有该类型的组件但值不同，则粘贴组件值
                    var destComponent = obj.GetComponent(comType);
                    if (destComponent != null)
                    {
                        ComponentUtility.CopyComponent(srcComponent);
                        ComponentUtility.PasteComponentValues(destComponent);
                        Debug.Log($"srcComponent == destComponent {srcComponent == destComponent}");
                    }
                }
            }), kv.Key);
        }
        menu.ShowAsContext();
    }

    static void OnComponentSelected(object arg)
    {
        var component = (Component) arg;
        srcComponentsDic[component.GetType()] = component;
    }

    static void OnSequenceSelected(object arg)
    {
        var menuComponent = (MenuComponent) arg;
        if (null != menuComponent)
        {
            menuComponent.isCheck = !menuComponent.isCheck;
            if (menuComponent.Name == k_All)
            {
                foreach (var m in menuComponents)
                {
                    if (m.Component != null && !srcComponents.Contains(m.Component))
                        srcComponents.Add(m.Component);
                }
            }
            else if (menuComponent.Name == k_None)
            {
                var destComponents = new List<Component>();
                foreach (var m in menuComponents)
                {
                    if (m.Component != null)
                        destComponents.Add(m.Component);
                }

                srcComponents = RemoveDuplicatesFromTwoLists(destComponents, srcComponents);
            }
            else
            {
                if (menuComponent.isCheck && !srcComponents.Contains((menuComponent.Component)))
                {
                    srcComponents.Add(menuComponent.Component);
                }
                else if (!menuComponent.isCheck && srcComponents.Contains((menuComponent.Component)))
                {
                    srcComponents.Remove(menuComponent.Component);
                }
            }
        }
    }
    
    public static List<T> RemoveDuplicatesFromTwoLists<T>(List<T> list1, List<T> list2)
    {
        // 创建一个只包含唯一元素的新列表，移除了第二个列表中存在的任何元素
        List<T> uniqueList = list1.Except(list2).ToList();

        // 返回结果
        return uniqueList;
    }
    
    public static List<Component> RemoveDuplicates(List<Component> components)
    {
        // 使用 Distinct 来移除重复的组件。需要注意的是，Distinct 方法会根据组件的引用来比较是否相同，
        // 所以确保你希望的行为就是移除相同引用的实例。
        return components.Distinct().ToList();
    }

    class MenuComponent
    {
        public string Name;
        public Component Component;
        public bool isCheck = true;

        public MenuComponent(string name, Component component)
        {
            Name = name;
            Component = component;
        }
    }
}
