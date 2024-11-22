using System.Collections.Generic;
using System.Linq;
using EBA.Ebunieditor.Editor.Common;
using UnityEditor;
using UnityEngine;

namespace EBUniEditor.Editor.Hierarchy
{
    [InitializeOnLoad]
    public static class RectTransformHierarchyDrawer
    {
        private static Dictionary<int, RectTransform> selectedRects = new Dictionary<int, RectTransform>();

        static RectTransformHierarchyDrawer()
        {
            // 添加绘制回调函数
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static bool IsMissComponent(GameObject go)
        {
            if (go == null)
                return false;
            var components = go.GetComponents<Component>();
            return components.Any(component => component == null);
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (!GlobalScriptableObject.instance.isShowRectDrawer)
                return;
            // 获取对象对应的GameObject
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj == null)
                return;
            // 获取GameObject的RectTransform组件
            var rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;
            // 定义复选框的区域和状态
            var toggleRect = new Rect(selectionRect.x + selectionRect.width - 20, selectionRect.y, 18, selectionRect.height);
            var isSelected = selectedRects.ContainsKey(instanceID);
            // 绘制复选框
            var shouldBeSelected = GUI.Toggle(toggleRect, isSelected, GUIContent.none);
            
            if (IsMissComponent(obj))
            {
                var missRect = new Rect(selectionRect.x + selectionRect.width, selectionRect.y, 18, selectionRect.height);
                GUI.Label(missRect, "⚠️");
            }
            
            if (shouldBeSelected == isSelected)
                return;
            if (shouldBeSelected)
            {
                // 添加到字典
                selectedRects.Add(instanceID, rectTransform);
            }
            else
            {
                // 从字典中移除
                selectedRects.Remove(instanceID);
            }
        }
    
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!GlobalScriptableObject.instance.isShowRectDrawer)
                return;
            Handles.color = Color.green;
            foreach (var target in selectedRects)
            {
                if (!target.Value)
                    continue;
                var t = target.Value;
                var position = t.position;
                var localPosition = t.localPosition;
                var sizeDelta = t.sizeDelta;
                var pivot = t.pivot;
                var lossyScale = t.lossyScale;
        
                var xOffset = position.x - localPosition.x;
                var yOffset = position.y - localPosition.y;
        
                var x1 = (localPosition.x - (sizeDelta.x * pivot.x) * lossyScale.x) + xOffset;
                var x2 = (localPosition.x + (sizeDelta.x * (1f - pivot.x) * lossyScale.x)) + xOffset;
                var y1 = (localPosition.y - (sizeDelta.y * pivot.y) * lossyScale.y) + yOffset;
                var y2 = (localPosition.y + (sizeDelta.y * (1f - pivot.y) * lossyScale.y)) + yOffset;
        
                const float max = 100000f;
                const float min = -100000f;
        
                Gizmos.color = Color.green;
                Handles.DrawLine(new Vector3(x1, min, 0f), new Vector3(x1, max, 0f));
                Handles.DrawLine(new Vector3(x2, min, 0f), new Vector3(x2, max, 0f));
                Handles.DrawLine(new Vector3(min, y1, 0f), new Vector3(max, y1, 0f));
                Handles.DrawLine(new Vector3(min, y2, 0f), new Vector3(max, y2, 0f));
            }
        }
    
    }
}