using UnityEditor;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Common
{
    public class BaseScriptable<T>: ScriptableObject where T: ScriptableObject
    {
        private static T _instance;

        public static T Instance => instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    var name = typeof(T).Name;
                    var path = string.Format("Assets/SO/{0}.asset", name);
                    if (AssetImporter.GetAtPath("Assets/SO") == null)
                    {
                        AssetDatabase.CreateFolder("Assets", "SO");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    _instance = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (_instance == null)
                    {
                        _instance = CreateInstance<T>();
                        _instance.name = name;
                        AssetDatabase.CreateAsset(_instance, path);
                    }
                }

                return _instance;
            }
        }
    }
}