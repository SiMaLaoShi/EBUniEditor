using System;
using System.Collections.Generic;
using System.IO;
using EBA.LitJson;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EBA.Ebunieditor.Editor.Shortcut
{
    [Serializable]
    internal class PrefabEntity
    {
        public GameObject GameObject { get; set; }
        public string AssetPath { get; set; }
        public Texture PreviewTexture { get; set; }
    }

    internal class CacheEntity
    {
        public string AssetPath { get; set; }
        public string PreviewPath { get; set; }
    }

    public class PrefabBookmarkWindow : EditorWindow
    {
        [SerializeField] private List<PrefabEntity> prefabs = new List<PrefabEntity>();
        private Vector2 scrollPos;
        private const int CONST_ITEM_SIZE = 100;
        private const string CONST_BOOKMARK_CACHE_JSON = "PrefabBookmark.json";
        private const string CONST_BOOKMARK_DIRECTORY_NAME = "EBA_Cache";

        private static readonly string s_BookmarkDirectoryPath = Path.Combine(Environment.CurrentDirectory, CONST_BOOKMARK_DIRECTORY_NAME);

        private static readonly string s_BookmarkCacheJsonPath = Path.Combine(s_BookmarkDirectoryPath, CONST_BOOKMARK_CACHE_JSON);

        [MenuItem("Window/EBAWindow/PrefabBookmarkWindow")]
        public static void ShowWindow()
        {
            GetWindow<PrefabBookmarkWindow>("PrefabBookmarkWindow");
        }

        private void OnEnable()
        {
            LoadBookmark();
        }

        private void OnDisable()
        {
            SaveBookmark();
            prefabs = null;
        }

        private void OnDestroy()
        {
            SaveBookmark();
            prefabs = null;
        }

        #region 获取GameObject的预览图

        public static Bounds GetBounds(GameObject obj)
        {
            var min = new Vector3(99999, 99999, 99999);
            var max = new Vector3(-99999, -99999, -99999);
            var renders = obj.GetComponentsInChildren<MeshRenderer>();
            if (renders.Length > 0)
            {
                for (var i = 0; i < renders.Length; i++)
                {
                    if (renders[i].bounds.min.x < min.x)
                        min.x = renders[i].bounds.min.x;
                    if (renders[i].bounds.min.y < min.y)
                        min.y = renders[i].bounds.min.y;
                    if (renders[i].bounds.min.z < min.z)
                        min.z = renders[i].bounds.min.z;

                    if (renders[i].bounds.max.x > max.x)
                        max.x = renders[i].bounds.max.x;
                    if (renders[i].bounds.max.y > max.y)
                        max.y = renders[i].bounds.max.y;
                    if (renders[i].bounds.max.z > max.z)
                        max.z = renders[i].bounds.max.z;
                }
            }
            else
            {
                var rectTrans = obj.GetComponentsInChildren<RectTransform>();
                var corner = new Vector3[4];
                for (var i = 0; i < rectTrans.Length; i++)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    rectTrans[i].GetWorldCorners(corner);
                    if (corner[0].x < min.x)
                        min.x = corner[0].x;
                    if (corner[0].y < min.y)
                        min.y = corner[0].y;
                    if (corner[0].z < min.z)
                        min.z = corner[0].z;

                    if (corner[2].x > max.x)
                        max.x = corner[2].x;
                    if (corner[2].y > max.y)
                        max.y = corner[2].y;
                    if (corner[2].z > max.z)
                        max.z = corner[2].z;
                }
            }

            var center = (min + max) / 2;
            var size = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
            return new Bounds(center, size);
        }

        public static Texture GetAssetPreview(GameObject obj)
        {
            GameObject canvasObj = null;
            var clone = Instantiate(obj);
            var cloneTransform = clone.transform;
            var isUINode = false;
            if (cloneTransform is RectTransform)
            {
                //如果是UGUI节点的话就要把它们放在Canvas下了
                canvasObj = new GameObject("render canvas", typeof(Canvas));
                cloneTransform.SetParent(canvasObj.transform);
                cloneTransform.localPosition = Vector3.zero;

                canvasObj.transform.position = new Vector3(-1000, -1000, -1000);
                canvasObj.layer = 21; //放在21层，摄像机也只渲染此层的，避免混入了奇怪的东西
                isUINode = true;
            }
            else
            {
                cloneTransform.position = new Vector3(-1000, -1000, -1000);
            }

            var all = clone.GetComponentsInChildren<Transform>();
            foreach (var trans in all)
                trans.gameObject.layer = 21;

            var bounds = GetBounds(clone);
            var min = bounds.min;
            var max = bounds.max;
            var cameraObj = new GameObject("render camera");

            var renderCamera = cameraObj.AddComponent<Camera>();
            renderCamera.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            renderCamera.clearFlags = CameraClearFlags.Color;
            renderCamera.cameraType = CameraType.Preview;
            renderCamera.cullingMask = 1 << 21;
            if (isUINode)
            {
                var position = cloneTransform.position;
                cameraObj.transform.position =
                    new Vector3((max.x + min.x) / 2f, (max.y + min.y) / 2f, position.z - 100);
                var center = new Vector3(position.x + 0.01f, (max.y + min.y) / 2f,
                    position.z); //+0.01f是为了去掉Unity自带的摄像机旋转角度为0的打印，太烦人了
                cameraObj.transform.LookAt(center);

                renderCamera.orthographic = true;
                var width = max.x - min.x;
                var height = max.y - min.y;
                var maxCameraSize = width > height ? width : height;
                renderCamera.orthographicSize = maxCameraSize / 2; //预览图要尽量少点空白
            }
            else
            {
                cameraObj.transform.position =
                    new Vector3((max.x + min.x) / 2f, (max.y + min.y) / 2f, max.z + (max.z - min.z));
                var position = cloneTransform.position;
                var center = new Vector3(position.x + 0.01f, (max.y + min.y) / 2f,
                    position.z);
                cameraObj.transform.LookAt(center);

                var angle = (int) (Mathf.Atan2((max.y - min.y) / 2, max.z - min.z) * 180 / 3.1415f * 2);
                renderCamera.fieldOfView = angle;
            }

            var texture = new RenderTexture(128, 128, 0, RenderTextureFormat.Default);
            renderCamera.targetTexture = texture;

            Undo.DestroyObjectImmediate(cameraObj);
            Undo.PerformUndo(); //不知道为什么要删掉再Undo回来后才Render得出来UI的节点，3D节点是没这个问题的，估计是Canvas创建后没那么快有效？
            renderCamera.RenderDontRestore();
            var tex = new RenderTexture(128, 128, 0, RenderTextureFormat.Default);
            Graphics.Blit(texture, tex);

            DestroyImmediate(canvasObj);
            DestroyImmediate(cameraObj);
            return tex;
        }

        public static bool SaveTextureToPNG(Texture inputTex, string saveFileName)
        {
            var temp =
                RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(inputTex, temp);
            var ret = SaveRenderTextureToPNG(temp, saveFileName);
            RenderTexture.ReleaseTemporary(temp);
            return ret;
        }

        public static bool SaveRenderTextureToPNG(RenderTexture rt, string saveFileName)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            var bytes = png.EncodeToPNG();
            var directory = Path.GetDirectoryName(saveFileName);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var file = File.Open(saveFileName, FileMode.Create);
            var writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();
            DestroyImmediate(png);
            RenderTexture.active = prev;
            return true;
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            // 使用BeginVertical来保持代码结构清晰
            GUILayout.BeginVertical();
            var backgroundColor = GUI.backgroundColor; // 保存原来的背景颜色
            GUI.backgroundColor = Color.gray; // 设置背景颜色为灰色
            var windowWidth = position.width; // EditorWindow的当前宽度

            // 计算每行可以容纳的项目数，确保至少能容纳一个项目
            var itemsPerRow = Mathf.Max(1, Mathf.FloorToInt((windowWidth + 10) / (CONST_ITEM_SIZE + 10)));

            // 使用更新后的itemsPerRow来计算所需的行数
            var row = Mathf.CeilToInt(prefabs.Count / (float) itemsPerRow);
            for (var r = 0; r < row; r++)
            {
                GUILayout.BeginHorizontal(); // 开始一行
                for (var i = 0; i < itemsPerRow; i++)
                {
                    var index = r * itemsPerRow + i; // 计算当前索引
                    if (index < prefabs.Count)
                    {
                        DrawPrefabItem(prefabs[index], index);
                        GUILayout.Space(10);
                    }
                }

                GUILayout.Space(10);
                GUILayout.EndHorizontal(); // 结束一行
            }

            GUI.backgroundColor = backgroundColor; // 恢复原来的背景颜色
            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            DragAndDropGUI();
        }

        private void DrawPrefabItem(PrefabEntity p, int index)
        {
            var prefab = p.GameObject;
            GUILayout.BeginVertical(GUILayout.Width(CONST_ITEM_SIZE),
                GUILayout.Height(CONST_ITEM_SIZE + 20)); // 额外的20像素用于放置下方的标签或按钮
            var previewImage = p.PreviewTexture;
            var rect = GUILayoutUtility.GetRect(CONST_ITEM_SIZE, CONST_ITEM_SIZE, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(rect, previewImage ? previewImage : Texture2D.whiteTexture);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] {prefab};
                DragAndDrop.StartDrag(prefab.name);
                Event.current.Use(); // 阻止事件进一步传播
            }

            GUILayout.Label(prefab.name, EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button("Remove"))
                prefabs.RemoveAt(index);

            if (GUILayout.Button("Pin"))
                EditorGUIUtility.PingObject(prefab.gameObject);

            GUILayout.EndVertical();
        }

        private void DragAndDropGUI()
        {
            var evt = Event.current;
            var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag Prefab Here");

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            var go = draggedObject as GameObject;
                            if (go && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
                            {
                                var assetPath = AssetDatabase.GetAssetPath(go);
                                //todo 用字典
                                var isExit = false;
                                foreach (var entity in prefabs)
                                    if (entity.AssetPath == assetPath)
                                    {
                                        isExit = true;
                                        break;
                                    }

                                if (!isExit)
                                {
                                    var texture = GetAssetPreview(go);
                                    var item = new PrefabEntity
                                    {
                                        AssetPath = assetPath,
                                        GameObject = go,
                                        PreviewTexture = texture
                                    };
                                    prefabs.Add(item);
                                }
                            }
                        }
                    }

                    break;
            }
        }

        #endregion

        private void SaveBookmark()
        {
            if (null == prefabs)
                return;
            var cacheEntities = new List<CacheEntity>();
            if (!Directory.Exists(s_BookmarkDirectoryPath))
                Directory.CreateDirectory(s_BookmarkDirectoryPath);
            foreach (var prefab in prefabs)
            {
                var pngPath = Path.Combine(s_BookmarkDirectoryPath, Path.ChangeExtension(prefab.GameObject.name, "png"));
                if (!File.Exists(pngPath))
                    SaveTextureToPNG(prefab.PreviewTexture, pngPath);
                cacheEntities.Add(new CacheEntity
                {
                    AssetPath = prefab.AssetPath,
                    PreviewPath = pngPath
                });
            }

            //todo 不依赖第三方库
            var jw = new JsonWriter
            {
                PrettyPrint = true
            };
            JsonMapper.ToJson(cacheEntities, jw);
            File.WriteAllText(s_BookmarkCacheJsonPath, jw.ToString());
        }

        private static Texture FileToPng(string path)
        {
            //todo 这里不把Png放到Unity里面可能在GC的时候会直接丢掉
            if (!File.Exists(path))
                return null;
            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(CONST_ITEM_SIZE, CONST_ITEM_SIZE);
            texture.LoadImage(bytes);

            return null;
        }

        private void LoadBookmark()
        {
            if (!File.Exists(s_BookmarkCacheJsonPath))
                return;
            var cacheEntities = JsonMapper.ToObject<List<CacheEntity>>(File.ReadAllText(s_BookmarkCacheJsonPath));
            prefabs.Clear();
            foreach (var p in cacheEntities)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(p.AssetPath);
                if (null == obj)
                    continue;
                var texture = FileToPng(p.PreviewPath);
                if (null == texture)
                    texture = GetAssetPreview(obj);

                prefabs.Add(new PrefabEntity
                {
                    GameObject = obj,
                    AssetPath = p.AssetPath,
                    PreviewTexture = texture
                });
            }
        }
    }
}