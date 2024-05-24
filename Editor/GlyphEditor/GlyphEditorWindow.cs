using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EBA.Ebunieditor.Editor.GlyphEditor
{
    [Serializable]
    public class Glyph
    {
        public Sprite Sprite; // 美术字
        public string Index; // 文字索引
    }

    [Serializable]
    public class GlyphInfo
    {
        public string index;
        public Rect uv;
        public Rect vert;
    }

    public class GlyphEditorWindow : EditorWindow
    {
        private List<Glyph> glyphList = new List<Glyph>();
        private Vector2 scrollPos;

        [MenuItem("Window/EBAWindow/GlyphEditorWindow")]
        public static void ShowWindow()
        {
            GetWindow<GlyphEditorWindow>("GlyphEditorWindow");
        }

        private string fontName = "Default_Font";

        public static bool TryConvertGlyphIndex(string glyphIndex, out int index)
        {
            index = 0; // 初始化输出参数

            try
            {
                var bytes = Encoding.Unicode.GetBytes(glyphIndex);
                var stringBuilder = new StringBuilder();
                for (var j = 0; j < bytes.Length; j += 2)
                {
                    stringBuilder.AppendFormat("{0:x2}{1:x2}", bytes[j + 1], bytes[j]);
                }

                index = Convert.ToInt32(stringBuilder.ToString(), 16);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{glyphIndex}转换异常: {ex.Message}");
                return false;
            }
        }

        private GlyphInfoScriptableObject glyphInfoScriptableObject;

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            glyphInfoScriptableObject = (GlyphInfoScriptableObject) EditorGUILayout.ObjectField("Select Object:", glyphInfoScriptableObject, typeof(GlyphInfoScriptableObject), false);
            if (GUILayout.Button("编辑旧字体"))
            {
                glyphList.Clear();
                glyphList = glyphInfoScriptableObject.GlyphInfos;
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("添加新元素"))
                glyphList.Add(new Glyph());

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            for (var i = 0; i < glyphList.Count; i++)
            {
                GUILayout.BeginHorizontal();

                glyphList[i].Sprite = (Sprite) EditorGUILayout.ObjectField("Sprite:", glyphList[i].Sprite, typeof(Sprite), false);
                glyphList[i].Index = EditorGUILayout.TextField("文字索引:", glyphList[i].Index);

                if (GUILayout.Button("删除"))
                    glyphList.RemoveAt(i);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            GUILayout.Label("字体名字", GUILayout.Width(100));
            fontName = GUILayout.TextField(fontName, GUILayout.Width(200));
            if (GUILayout.Button("生成"))
                Run();
            GUILayout.EndHorizontal();
        }

        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");

        private void Run()
        {
            if (string.IsNullOrEmpty(fontName))
            {
                Debug.LogError("没有输入字体名字");
                return;
            }

            var keys = new List<string>();

            for (var i = 0; i < glyphList.Count; i++)
            {
                var glyph = glyphList[i];
                if (null == glyph.Sprite)
                {
                    Debug.LogError($"第{i + 1}位置没有选择美术图片");
                    return;
                }

                if (string.IsNullOrEmpty(glyph.Index))
                {
                    Debug.LogError($"第{i + 1}位置没有配置关键字");
                    return;
                }

                if (keys.Contains(glyph.Index))
                {
                    Debug.LogError($"{glyph.Index}存在重复");
                    return;
                }

                if (!TryConvertGlyphIndex(glyph.Index, out var code))
                {
                    Debug.LogError($"不支持的key{glyph.Index}");
                    return;
                }

                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(glyph.Sprite.texture));
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
            }

            // 计算合并后图像的总宽度和高度
            var width = 0;
            var height = 0;

            foreach (var glyph in glyphList)
            {
                if (glyph.Sprite != null)
                {
                    width += (int) glyph.Sprite.rect.width;
                    height = Mathf.Max(height, (int) glyph.Sprite.rect.height);
                }
            }
                

            // 创建一个新的Texture2D来容纳所有纹理
            //todo 这个可用SpritePack来合并纹理
            var combinedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var clearColor = new Color[width * height];
            for (var i = 0; i < clearColor.Length; i++)
                clearColor[i] = Color.clear;
            combinedTexture.SetPixels(clearColor);

            // UV 信息列表
            var glyphInfos = new List<GlyphInfo>();

            // 将每个sprite写入新的纹理
            var offsetX = 0;
            foreach (var glyph in glyphList)
            {
                if (glyph.Sprite != null)
                {
                    var texture = glyph.Sprite.texture;
                    var spriteRect = glyph.Sprite.rect;
                    var pixels = texture.GetPixels(
                        (int) spriteRect.x,
                        (int) spriteRect.y,
                        (int) spriteRect.width,
                        (int) spriteRect.height
                    );

                    combinedTexture.SetPixels(
                        offsetX,
                        0,
                        (int) spriteRect.width,
                        (int) spriteRect.height,
                        pixels
                    );

                    // 记录 UV 和 Vert 信息
                    var uvInfo = new GlyphInfo
                    {
                        index = glyph.Index,
                        uv = new Rect(
                            (float) offsetX / width,
                            0,
                            spriteRect.width / (float) width,
                            spriteRect.height / (float) height
                        ),
                        vert = new Rect(
                            offsetX,
                            0,
                            spriteRect.width,
                            spriteRect.height
                        )
                    };

                    glyphInfos.Add(uvInfo);

                    offsetX += (int) spriteRect.width;
                }
            }
            
            combinedTexture.Apply();

            var bytes = combinedTexture.EncodeToPNG();
            if (!AssetDatabase.IsValidFolder("Assets/Fonts"))
            {
                AssetDatabase.CreateFolder("Assets", "Fonts");
            }

            if (!AssetDatabase.IsValidFolder($"Assets/Fonts/{fontName}"))
            {
                AssetDatabase.CreateFolder("Assets/Fonts", fontName);
            }

            var filePath = Path.Combine(Environment.CurrentDirectory, $"Assets/Fonts/{fontName}/{fontName}.png");
            if (!string.IsNullOrEmpty(filePath))
            {
                File.WriteAllBytes(filePath, bytes);
                Debug.Log("合并后的图像成功保存到: " + filePath);
            }

            var scriptableObject = CreateInstance<GlyphInfoScriptableObject>();
            scriptableObject.FontName = fontName;
            scriptableObject.GlyphInfos = glyphList;
            AssetDatabase.CreateAsset(scriptableObject, $"Assets/Fonts/{fontName}/{fontName}.asset");
            EditorUtility.SetDirty(scriptableObject);
            // 释放内存
            DestroyImmediate(combinedTexture);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GenerateFont(height, glyphInfos);
            
            for (var i = 0; i < glyphList.Count; i++)
            {
                var glyph = glyphList[i];
                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(glyph.Sprite.texture));
                textureImporter.isReadable = false;
                textureImporter.SaveAndReimport();
            }
        }

        private void GenerateAtlas()
        {
        }

        private void GenerateFont(float height, List<GlyphInfo> glyphInfos)
        {
            var font = new Font(fontName);
            var mat = new Material(Shader.Find("GUI/Text Shader"));
            var textureImporter = (TextureImporter) AssetImporter.GetAtPath($"Assets/Fonts/{fontName}/{fontName}.png");
            textureImporter.textureType = TextureImporterType.GUI;
            textureImporter.SaveAndReimport();
            var fontTexture = AssetDatabase.LoadAssetAtPath<Texture>($"Assets/Fonts/{fontName}/{fontName}.png");
            mat.SetTexture(s_MainTex, fontTexture);
            AssetDatabase.CreateAsset(mat, $"Assets/Fonts/{fontName}/{fontName}.mat");

            var so = new SerializedObject(font);
            var p = so.FindProperty("m_LineSpacing");
            p.floatValue = height;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(font, $"Assets/Fonts/{fontName}/{fontName}.fontsettings");
            font.material = mat;
            font.characterInfo = SetCharacterInfo(glyphInfos);

            EditorUtility.SetDirty(font);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private CharacterInfo[] SetCharacterInfo(List<GlyphInfo> glyphUVInfos)
        {
            var info = new CharacterInfo[glyphUVInfos.Count];
            for (var i = 0; i < glyphUVInfos.Count; i++)
            {
                var bytes = Encoding.Unicode.GetBytes(glyphUVInfos[i].index);
                var stringBuilder = new StringBuilder();
                for (var j = 0; j < bytes.Length; j += 2)
                    stringBuilder.AppendFormat("{0:x2}{1:x2}", bytes[j + 1], bytes[j]);
                var index = Convert.ToInt32(stringBuilder.ToString(), 16);
                info[i].index = index;
                var uv = glyphUVInfos[i].uv;
                var x = uv.x;
                var y = uv.y;
                var width = uv.width;
                var height = uv.height;
                var column = 1f / width;
                var row = 1f / height;
                info[i].uvBottomLeft = new Vector2(x, y);
                info[i].uvBottomRight = new Vector2(x + 1f / column, y);
                info[i].uvTopLeft = new Vector2(x, y + 1f / row);
                info[i].uvTopRight = new Vector2(1f / column + x, y + 1f / row);

                var vert = glyphUVInfos[i].vert;
                info[i].minX = 0;
                info[i].minY = (int) -vert.height;
                info[i].maxX = (int) vert.width;
                info[i].maxY = 0;
                info[i].advance = (int) vert.width;
            }

            return info;
        }
    }
}