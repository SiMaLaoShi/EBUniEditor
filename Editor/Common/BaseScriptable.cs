using UnityEditor;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Common
{
    public class BaseScriptable<T>: ScriptableObject where T: ScriptableObject
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    var name = typeof(T).Name;
                    var path = string.Format("Assets/SO/{0}.asset", name);
                    if (AssetImporter.GetAtPath("Assets/SO") == null)
                    {
                        AssetDatabase.CreateFolder("Assets", "SO");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    instance = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (instance == null)
                    {
                        instance = CreateInstance<T>();
                        instance.name = name;
                        AssetDatabase.CreateAsset(instance, path);
                    }
                }
                return instance;
            }
        }
    }
}