using System.Reflection;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace EBUniEditor.Editor.Inspector
{
    // [CustomEditor(typeof(TextureImporter)), CanEditMultipleObjects]
    public class TextureImporterInspectorExtension : DecoratorEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Button("111222");
            
            MethodInfo methodInfo = typeof(AssetImporterEditor).GetMethod("ApplyRevertGUI", BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo != null)
            {
                // 调用方法
                methodInfo.Invoke(target, new object[] { });
            }
            else
            {
                
            }
        }

        public TextureImporterInspectorExtension() : base("TextureImporterInspector")
        {
            
        }
    }
}