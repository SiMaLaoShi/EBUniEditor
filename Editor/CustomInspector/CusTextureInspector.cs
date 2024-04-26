using UnityEditor;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.CustomInspector
{
    // [CustomEditor(typeof(TextureImporter))]
    // [CanEditMultipleObjects]
    internal class CusTextureInspector : TextureImporterInspector
    {
        private SerializedProperty m_SpritePackingTagProperty;
        private SerializedProperty m_TextureTypeProperty;

        private readonly GUIContent spritePackingTag =
            EditorGUIUtility.TrTextContent("Packing Tag", "Tag for the Sprite Packing system.");

        public override void OnEnable()
        {
            base.OnEnable();
            m_SpritePackingTagProperty = serializedObject.FindProperty("m_SpritePackingTag");
            m_TextureTypeProperty = serializedObject.FindProperty("m_TextureType");
            m_spritePackingTag = m_SpritePackingTagProperty.stringValue;
            m_oldPackingTag = m_SpritePackingTagProperty.stringValue;
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        private string m_spritePackingTag = string.Empty;
        private string m_oldPackingTag = string.Empty;

        public override void OnInspectorGUI()
        {
            if (m_TextureTypeProperty.intValue == (int)TextureImporterType.Sprite)
                DrawPackingTag();
            base.OnInspectorGUI();
            DrawOtherButton();
        }

        private void DrawOtherButton()
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("设置压缩格式（Sprite ASTC_6x6）"))
                SetSpriteCompress();
            if (GUILayout.Button("设置压缩格式（Texture2D ASTC_6x6）"))
                SetSpineTexture2DCompress();
            if (GUILayout.Button("设置压缩格式（Texture2D ASTC_5x5）"))
                SetCharacterTexture2DCompress();
            EditorGUILayout.EndVertical();
        }

        private void SetSpriteCompress()
        {
            var guids = Selection.assetGUIDs;
            var count = 0;
            foreach (var guid in guids)
            {
                var imp = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
                CusTextureUtil.SetSpriteImporter(AssetDatabase.GUIDToAssetPath(guid), imp.spritePackingTag,
                    TextureImporterFormat.ASTC_6x6,
                    TextureImporterFormat.ASTC_6x6, 69);
                EditorUtility.DisplayProgressBar(guid, AssetDatabase.GUIDToAssetPath(guid),
                    (float)++count / guids.Length);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private void SetSpineTexture2DCompress()
        {
            var guids = Selection.assetGUIDs;
            var count = 0;
            foreach (var guid in guids)
            {
                var imp = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
                CusTextureUtil.SetTexture2DFormat(AssetDatabase.GUIDToAssetPath(guid), TextureImporterFormat.ASTC_6x6,
                    TextureImporterFormat.ASTC_6x6);
                if (imp.textureType != TextureImporterType.Default || imp.mipmapEnabled || imp.alphaIsTransparency)
                {
                    imp.textureType = TextureImporterType.Default;
                    imp.mipmapEnabled = false;
                    imp.alphaIsTransparency = false;
                    imp.SaveAndReimport();
                }

                EditorUtility.DisplayProgressBar(guid, AssetDatabase.GUIDToAssetPath(guid),
                    (float)++count / guids.Length);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private void SetCharacterTexture2DCompress()
        {
            var guids = Selection.assetGUIDs;
            var count = 0;
            foreach (var guid in guids)
            {
                var imp = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
                CusTextureUtil.SetTexture2DFormat(AssetDatabase.GUIDToAssetPath(guid), TextureImporterFormat.ASTC_5x5,
                    TextureImporterFormat.ASTC_5x5);
                if (imp.textureType != TextureImporterType.Default || imp.mipmapEnabled || imp.alphaIsTransparency)
                {
                    imp.textureType = TextureImporterType.Default;
                    imp.mipmapEnabled = false;
                    imp.alphaIsTransparency = false;
                    imp.SaveAndReimport();
                }

                EditorUtility.DisplayProgressBar(guid, AssetDatabase.GUIDToAssetPath(guid),
                    (float)++count / guids.Length);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private void DrawPackingTag()
        {
            GUILayout.BeginHorizontal();
            if (m_spritePackingTag == string.Empty)
                GUI.color = Color.red;
            GUILayout.Label(spritePackingTag, GUILayout.Width(100));
            m_spritePackingTag = EditorGUILayout.TextField(m_spritePackingTag);
            GUI.color = GUI.contentColor;
            GUILayout.EndHorizontal();
            if (m_SpritePackingTagProperty.stringValue != m_spritePackingTag && m_spritePackingTag != m_oldPackingTag)
            {
                m_SpritePackingTagProperty.serializedObject.Update();
                m_SpritePackingTagProperty.stringValue = m_spritePackingTag;
                m_SpritePackingTagProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnDestroy()
        {
            m_TextureTypeProperty = null;
            m_SpritePackingTagProperty = null;
        }
    }
}