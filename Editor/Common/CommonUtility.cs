using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using Object = System.Object;
using UObject = UnityEngine.Object;
using UnityEditor;

namespace EBA.Ebunieditor.Editor.Common
{
    public partial class CommonUtility
    {
        public static bool SetSpriteImporter(string path, string tag, TextureImporterFormat androidFormat,
            TextureImporterFormat iosFormat, int perUnit)
        {
            AssetImporter im = AssetImporter.GetAtPath(path);
            bool isRei = false;
            if (im as TextureImporter)
            {
                var tIm = im as TextureImporter;
                var androidSettings = tIm.GetPlatformTextureSettings("Android");
                if (androidSettings.format != androidFormat)
                {
                    isRei = true;
                    androidSettings.format = androidFormat;
                }

                if (!androidSettings.overridden)
                {
                    isRei = true;
                    androidSettings.overridden = true;
                }

                if (androidSettings.compressionQuality != 100)
                {
                    isRei = true;
                    androidSettings.compressionQuality = 100;
                }

                tIm.SetPlatformTextureSettings(androidSettings);
                var iosSettings = tIm.GetPlatformTextureSettings("iPhone");
                if (iosSettings.format != iosFormat)
                {
                    iosSettings.format = iosFormat;
                    isRei = true;
                }

                
                if (!iosSettings.overridden)
                {
                    isRei = true;
                    iosSettings.overridden = true;
                }

                if (tIm.spritePackingTag != tag)
                {
                    tIm.spritePackingTag = tag;
                    isRei = true;
                }

                if (tIm.textureType != TextureImporterType.Sprite)
                {
                    tIm.textureType = TextureImporterType.Sprite;
                    isRei = true;
                }

                if ((int) tIm.spritePixelsPerUnit != perUnit)
                {
                    tIm.spritePixelsPerUnit = perUnit;
                    isRei = true;
                }
                tIm.SetPlatformTextureSettings(iosSettings);
                if (isRei)
                    tIm.SaveAndReimport();
            }

            return isRei;
        }

        public static bool SetTexture2DFormat(string path,
            TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_6x6,
            TextureImporterFormat iosFormat = TextureImporterFormat.ASTC_6x6)
        {
            AssetImporter im = AssetImporter.GetAtPath(path);
            bool isRei = false;
            if (im as TextureImporter)
            {
                var tIm = im as TextureImporter;
                var androidSettings = tIm.GetPlatformTextureSettings("Android");
                if (androidSettings.format != androidFormat)
                {
                    isRei = true;
                    androidSettings.format = androidFormat;
                }

                if (!androidSettings.overridden)
                {
                    isRei = true;
                    androidSettings.overridden = true;
                }

                if (androidSettings.compressionQuality != 100)
                {
                    isRei = true;
                    androidSettings.compressionQuality = 100;
                }

                tIm.SetPlatformTextureSettings(androidSettings);
                var iosSettings = tIm.GetPlatformTextureSettings("iPhone");
                if (iosSettings.format != iosFormat)
                {
                    iosSettings.format = iosFormat;
                    isRei = true;
                }

                tIm.SetPlatformTextureSettings(iosSettings);
                if (!iosSettings.overridden)
                {
                    isRei = true;
                    iosSettings.overridden = true;
                }

                if (tIm.textureType != TextureImporterType.Sprite)
                {
                    tIm.textureType = TextureImporterType.Sprite;
                    isRei = true;
                }

                if (isRei)
                    tIm.SaveAndReimport();
            }

            return isRei;
        }

        public static T AttachUniqueComponent<T>(GameObject go) where T : Component
        {
            if ((UnityEngine.Object) null == (UnityEngine.Object) go)
                return default(T);
            T obj = go.GetComponent<T>();
            if ((UnityEngine.Object) null == (UnityEngine.Object) obj)
                obj = go.AddComponent<T>();
            return obj;
        }

        public const BindingFlags BindFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        public static void ClearProgress()
        {
            EditorUtility.ClearProgressBar();
        }

        public static string GetEBEditorUpmRootPath()
        {
            return Path.GetFullPath(@"Packages\com.eba.ebunieditor\");
        }

        public static void UpdateProgress(int progress, int progressMax, string desc)
        {
            var title = "Processing...[" + progress + " - " + progressMax + "]";
            var value = progress / (float) progressMax;
            EditorUtility.DisplayProgressBar(title, desc, value);
        }

        #region 设置GameView

        public enum GameViewSizeType
        {
            AspectRatio,
            FixedResolution
        };

        public static void SetGameView(int index)
        {
            var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var window = EditorWindow.GetWindow(type);
            var sizeSelectionCallbackMethod = type.GetMethod("SizeSelectionCallback",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            // ReSharper disable once PossibleNullReferenceException
            sizeSelectionCallbackMethod.Invoke(window, new object[] {index, null});
        }

        public static void SetGameView(int width, int height, string baseText = "",
            GameViewSizeType sizeType = GameViewSizeType.FixedResolution)
        {
            var sizesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            var getGroupMethod = sizesType.GetMethod("GetGroup");
            var getCurrentGroupTypeMethod = sizesType.GetMethod("get_currentGroupType");

            // ReSharper disable once PossibleNullReferenceException
            var gameViewSizesInstance = instanceProp.GetValue(null, null);

            // ReSharper disable once PossibleNullReferenceException
            var curGroupType = getCurrentGroupTypeMethod.Invoke(gameViewSizesInstance, new object[] { });

            // ReSharper disable once PossibleNullReferenceException
            var group = getGroupMethod.Invoke(gameViewSizesInstance, new object[] {(int) curGroupType});
            var getTotalCountMethod = group.GetType().GetMethod("GetTotalCount", BindFlags);
            // ReSharper disable once PossibleNullReferenceException
            var count = (int) getTotalCountMethod.Invoke(group, null);

            for (var i = 0; i < count; i++)
            {
                // ReSharper disable once PossibleNullReferenceException
                var viewSize = group.GetType().GetMethod("GetGameViewSize", BindFlags)
                    .Invoke(group, new object[] {i});
                // ReSharper disable once PossibleNullReferenceException
                var sWidth = (int) viewSize.GetType().GetField("m_Width", BindFlags).GetValue(viewSize);
                // ReSharper disable once PossibleNullReferenceException
                var sHeight = (int) viewSize.GetType().GetField("m_Height", BindFlags).GetValue(viewSize);
                if (width == sWidth && sHeight == height)
                {
                    SetGameView(i);
                    return;
                }
            }

            var param = new object[4];
            param[0] = (int) sizeType;
            param[1] = width;
            param[2] = height;
            param[3] = string.IsNullOrEmpty(baseText) ? "" + width + "x" + height : baseText;
            var size = typeof(UnityEditor.Editor).Assembly.CreateInstance("UnityEditor.GameViewSize", true, BindFlags,
                null, param,
                null,
                null);
            // ReSharper disable once PossibleNullReferenceException
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] {size});
            SetGameView(count);
        }

        #endregion

        #region 闪屏

        private static void SetSplashScreen()
        {
            SetSplashScreen(EditorGUIUtility.systemCopyBuffer);
        }

        /// <summary>
        ///     设置闪屏
        /// </summary>
        /// <param name="res">图片在Asset路径</param>
        public static void SetSplashScreen(string res)
        {
            //常规代码
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.backgroundColor = Color.white;
            PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.AllSequential;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            //PlayerSettings.SplashScreen.logos = null;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(res);
            var obj = typeof(PlayerSettings);
            var method = obj.GetMethod("FindProperty", BindFlags);

            // ReSharper disable once PossibleNullReferenceException
            var property = method.Invoke(obj, new object[] {"androidSplashScreen"}) as SerializedProperty;
            // ReSharper disable once PossibleNullReferenceException
            property.serializedObject.Update();
            property.objectReferenceValue = tex;
            property.serializedObject.ApplyModifiedProperties();

            property = method.Invoke(obj, new object[] {"iOSLaunchScreenPortrait"}) as SerializedProperty;
            // ReSharper disable once PossibleNullReferenceException
            property.serializedObject.Update();
            property.objectReferenceValue = tex;
            property.serializedObject.ApplyModifiedProperties();

            property = method.Invoke(obj, new object[] {"m_VirtualRealitySplashScreen"}) as SerializedProperty;
            // ReSharper disable once PossibleNullReferenceException
            property.serializedObject.Update();
            property.objectReferenceValue = tex;
            property.serializedObject.ApplyModifiedProperties();

            property = method.Invoke(obj, new object[] {"iOSLaunchScreenLandscape"}) as SerializedProperty;
            // ReSharper disable once PossibleNullReferenceException
            property.serializedObject.Update();
            property.objectReferenceValue = tex;
            property.serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion

        #region 层次面板快捷操作

        /// <summary>
        ///     拷贝Project视图中的某个Prefab
        /// </summary>
        public void ProjectDuplicate()
        {
            var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectWindowUtil");
            var method = type.GetMethod("DuplicateSelectedAssets", BindFlags);
            // ReSharper disable once PossibleNullReferenceException
            method.Invoke(null, null);
        }

        /// <summary>
        ///     拷贝Scene视图的一个GameObject和Ctrl+D一致
        /// </summary>
        public void HierarchyDuplicate()
        {
            var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var window = EditorWindow.GetWindow(type);
            var method = type.GetMethod("DuplicateGO", BindFlags);
            // ReSharper disable once PossibleNullReferenceException
            method.Invoke(window, null);
        }

        #endregion

        #region 操作prefab

        /// <summary>
        ///     apply一个GameObject
        /// </summary>
        /// <param name="assetPath"></param>
        public static void ApplyPrefab(string assetPath)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            EditorApplication.isPaused = false;
            ApplyPrefab(go);
        }

        /// <summary>
        ///     apply 一个GameObject
        /// </summary>
        /// <param name="go"></param>
        public static void ApplyPrefab(GameObject go)
        {
            var bak = new GameObject("@bak");
            var obj = PrefabUtility.InstantiatePrefab(go) as GameObject;
            if (obj != null)
            {
                obj.transform.SetParent(bak.transform, true);
                PrefabApply(obj);
            }

            UObject.DestroyImmediate(bak);
        }

        private static void PrefabApply(GameObject obj)
        {
            var pType = PrefabUtility.GetPrefabType(obj);
            if (pType != PrefabType.PrefabInstance)
                return;

            //这里必须获取到prefab实例的根节点，否则ReplacePrefab保存不了
            //GameObject prefabGo = GetPrefabInstanceParent(obj);
            var prefabGo = obj;
            if (prefabGo != null)
            {
                var prefabAsset = PrefabUtility.GetPrefabParent(prefabGo);
                if (prefabAsset != null)
                    PrefabUtility.ReplacePrefab(prefabGo, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
            }

            AssetDatabase.SaveAssets();
        }

        #endregion


        /// <summary>
        ///     获取父节点下所有子节点
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="transforms">容器</param>
        public static void GetAllChild(Transform parent, ref List<Transform> transforms)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var tr = parent.GetChild(i);
                transforms.Add(tr);
                if (tr.childCount > 0)
                    GetAllChild(tr, ref transforms);
            }
        }

        /// <summary>
        ///     获取一个一个子节点相对于父节点的路径
        /// </summary>
        /// <param name="root"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetTransformPath(Transform root, Transform node)
        {
            if (null == node || null == root)
                return string.Empty;
            var sb = node.name;
            while (node.name != root.name && null != node.parent)
            {
                node = node.parent;
                sb = node.name + "/" + sb;
            }

            return sb;
        }

        public static void GetNodePath(Transform trans, ref string path)
        {
            if (path == "")
            {
                path = trans.name;
            }
            else
            {
                path = trans.name + "/" + path;
            }

            if (trans.parent != null)
            {
                GetNodePath(trans.parent, ref path);
            }
        }

        #region 拷贝路径

        [MenuItem("Assets/Copy Utility/AssetBundle TAG", false, 1)]
        public static void CopyBundleName()
        {
            var guids = Selection.assetGUIDs;
            var str = "";
            foreach (var guid in guids)
            {
                var assetImporter = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                if (null != assetImporter)
                    str += assetImporter.assetBundleName;
            }

            GUIUtility.systemCopyBuffer = str;
        }

        [MenuItem("Assets/Copy Utility/GUID", false, 1)]
        public static void CopyGuid()
        {
            var guids = Selection.assetGUIDs;
            var str = "";
            foreach (var guid in guids) str += guid;

            GUIUtility.systemCopyBuffer = str;
        }

        [MenuItem("Assets/Copy Utility/AssetPath", false, 1)]
        public static void CopyAssetPath()
        {
            var guids = Selection.assetGUIDs;
            var str = "";
            foreach (var guid in guids) str += AssetDatabase.GUIDToAssetPath(guid);
            GUIUtility.systemCopyBuffer = str;
        }

        [MenuItem("Assets/Copy Utility/FilePath", false, 1)]
        public static void CopyFilePath()
        {
            var guids = Selection.assetGUIDs;
            var str = "";
            foreach (var guid in guids)
                str += string.Format("{0}/{1}", Environment.CurrentDirectory, AssetDatabase.GUIDToAssetPath(guid));
            GUIUtility.systemCopyBuffer = str;
        }

        #endregion
        
        [MenuItem(MenuKey.DEVELOPER_MODE)]
        private static void ToggleDevMode()
        {
            // 切换 Developer Mode 状态
            var isDevModeOn = !EditorPrefs.GetBool("DeveloperMode", false);
            EditorPrefs.SetBool("DeveloperMode", isDevModeOn);
            Menu.SetChecked(MenuKey.DEVELOPER_MODE, isDevModeOn);
        }
        
        [MenuItem(MenuKey.DEVELOPER_MODE, true)]
        private static bool ToggleDevModeValidate()
        {
            Menu.SetChecked(MenuKey.DEVELOPER_MODE, EditorPrefs.GetBool("DeveloperMode", false));
            return true; // 始终返回true以确保菜单项总是可用
        }

        public static List<string> GetSelectionAssetPaths(bool filterMeta = false)
        {
            var assetPaths = new List<string>();
            // 这个接口才能取到两列模式时候的文件夹
            foreach (var guid in Selection.assetGUIDs)
            {
                if (string.IsNullOrEmpty(guid))
                    continue;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    assetPaths.Add(path);
                    if (!filterMeta)
                        assetPaths.Add(path + ".meta");
                }
            }

            return assetPaths;
        }

        /// <summary>
        ///     对齐UI相机的UI
        /// </summary>
        public static void AlignSvToUi()
        {
            //todo 这里要修改自己的节点
            var cam = GameObject.Find("UGuiSystem").GetComponent<Camera>();
            var sv = SceneView.lastActiveSceneView;
            var svc = sv.camera;
            svc.nearClipPlane = cam.nearClipPlane;
            svc.farClipPlane = cam.farClipPlane;
            sv.size = Mathf.Sqrt(svc.aspect) / 0.1071068f;
            sv.pivot = cam.transform.position;
            sv.rotation = cam.transform.rotation;
            sv.orthographic = true;
            sv.Repaint();
        }

        public static void GUIDToAssetPath(string guid)
        {
            GUIUtility.systemCopyBuffer = AssetDatabase.GUIDToAssetPath(guid);
        }

        public static void AssetPathToGUID(string assetPath)
        {
            GUIUtility.systemCopyBuffer = AssetDatabase.AssetPathToGUID(assetPath);
        }

        public static void FindBigPicture()
        {
            var guids = AssetDatabase.FindAssets("t:texture");
            int count = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Texture texture2D = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guid));
                if (texture2D && texture2D.width > 1024 && texture2D.height > 1024)
                    Debug.Log(path);
                UpdateProgress(count, guids.Length, path);
                count++;
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 通过.colors文件获取一个color的List
        /// </summary>
        /// <param name="assetPath">资产路径</param>
        /// <returns></returns>
        public static List<Color> LoadColorPresetLib(string assetPath)
        {
            var path = Path.Combine(Environment.CurrentDirectory, assetPath);
            UnityEngine.Object[] instanceArray = InternalEditorUtility.LoadSerializedFileAndForget(path);
            UObject colorLibInstance = instanceArray[0];
            Type typeColorPresetLibrary = Type.GetType("UnityEditor.ColorPresetLibrary,UnityEditor");
            List<Color> colors = new List<Color>();
            if (typeColorPresetLibrary != null)
            {
                MethodInfo countFunc = typeColorPresetLibrary.GetMethod("Count");
                MethodInfo getNameFunc = typeColorPresetLibrary.GetMethod("GetName");
                MethodInfo getPresetFunc = typeColorPresetLibrary.GetMethod("GetPreset");
                if (null != colorLibInstance)
                {
                    if (countFunc != null)
                    {
                        int count = (int) countFunc.Invoke(colorLibInstance, null);
                        for (int i = 0; i < count; ++i)
                        {
                            if (getPresetFunc != null)
                            {
                                Color col = (Color) getPresetFunc.Invoke(colorLibInstance, new Object[1] {i});
                                colors.Add(col);
                            }
                        }
                    }
                }

                return colors;
            }

            return colors;
        }

        // 使用自定义格式化
        public static string FormatVector3(Vector3 vector, string format = "({0:F2}, {1:F2}, {2:F2})")
        {
            return string.Format(format, vector.x, vector.y, vector.z);
        }

        public static string FormatVector2(Vector2 vector, string format = "({0:F2}, {1:F2})")
        {
            return string.Format(format, vector.x, vector.y);
        }

        public static string FormatVector4(Vector4 vector, string format = "({0:F2}, {1:F2}, {2:F2}, {3:F3})")
        {
            return string.Format(format, vector.x, vector.y, vector.z, vector.w);
        }
    }
}