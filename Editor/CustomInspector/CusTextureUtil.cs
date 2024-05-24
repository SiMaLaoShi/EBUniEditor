using UnityEditor;

namespace EBA.Ebunieditor.Editor.CustomInspector
{
    public class CusTextureUtil
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

                tIm.SetPlatformTextureSettings(iosSettings);
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

                if (isRei)
                    tIm.SaveAndReimport();
            }

            return isRei;
        }

        public static bool SetTexture2DFormat(string path, TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_6x6,
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
    }
}