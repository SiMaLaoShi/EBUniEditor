using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using EBA.Ebunieditor.Editor.Common;
using MonoHook;
using UnityEditor;
using UnityEngine;
namespace EBA.Ebunieditor.Editor.EditorHook
{
#if UNITY_EDITOR
    public class ApplicationHook
    {
        public static string oldStreamingAssetsPath = Application.streamingAssetsPath;
        public static string oldPersistentDataPath = Application.persistentDataPath;

        public static string streamingAssetsPath
        {
            get
            {
                if (!GlobalScriptableObject.Instance.isHookStreamingAssetsPath)
                    return oldStreamingAssetsPath;
                var p = GlobalScriptableObject.Instance.strRemoteStreamingAssetsPath;
                return p == "" ? oldStreamingAssetsPath : p;
            }
        }

        public static string persistentDataPath
        {
            get
            {
                if (!GlobalScriptableObject.Instance.isHookPersistentDataPath)
                    return oldPersistentDataPath;
                var p = GlobalScriptableObject.Instance.strRemotePersistentDataPath;
                return p == "" ? oldPersistentDataPath : p;
            }
        }

        [Serializable]
        class HookInfo
        {
            public string OldMethod;
            public string NewMethod;
            public string ProxyMethod;
            public Type SrcType;
            public Type DestType;
        }
        static BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                                    BindingFlags.Instance;

        private static List<MethodHook> MethodHooks = new List<MethodHook>();
#if UNITY_5 || UNITY_2017_1_OR_NEWER
        //这个属性在场景加载后直接启动我们的方法
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void OnStartGame()
        {
            EditorApplication.playModeStateChanged += OnPlayerModeStateChanged;
            if (!GlobalScriptableObject.Instance.isHookApplication)
                return;

            List<HookInfo> hookInfos = new List<HookInfo>()
            {
                new HookInfo()
                {
                    OldMethod = "get_streamingAssetsPath",
                    NewMethod = "get_streamingAssetsPath",
                    ProxyMethod = "GetStreamingAssetsPathProxy",
                    SrcType = typeof(Application),
                    DestType = typeof(ApplicationHook),
                },
                new HookInfo()
                {
                    OldMethod = "get_persistentDataPath",
                    NewMethod = "get_persistentDataPath",
                    ProxyMethod = "GetPersistentDataPathProxy",
                    SrcType = typeof(Application),
                    DestType = typeof(ApplicationHook),
                },
            };
            
            foreach (var hookInfo in hookInfos)
            {
                var target = hookInfo.SrcType.GetMethod(hookInfo.OldMethod, bindingFlags);
                var dest = hookInfo.DestType.GetMethod(hookInfo.NewMethod, bindingFlags);
                var proxy = hookInfo.DestType.GetMethod(hookInfo.ProxyMethod, bindingFlags);
                var hook = new MethodHook(target, dest, proxy);
                hook.Install();
                MethodHooks.Add(hook);
            } 
            Debug.Log("Application.persistentDataPath:" + Application.persistentDataPath);
            Debug.Log("Application.streamingAssetsPath:" + Application.streamingAssetsPath);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static string GetStreamingAssetsPathProxy()
        {
            return streamingAssetsPath;
        }
        
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static string GetPersistentDataPathProxy()
        {
            return persistentDataPath;
        }

        static void OnPlayerModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
            {
                foreach (var methodHook in MethodHooks)
                {
                    methodHook.Uninstall();
                }
            }
        }
    }
#endif
}