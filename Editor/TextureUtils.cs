using System.IO;
using UnityEditor;
using UnityEngine;

namespace JellyTools.JellySceneResourcesReport
{
    public class TextureUtils
    {
        public static void ApplyTextureCompression(string path, TextureImporterFormat format,
            TextureImporterCompression compression)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                var def = importer.GetDefaultPlatformTextureSettings();
                def.textureCompression = compression;
                def.format = format;
                importer.ClearPlatformTextureSettings("iPhone");
                importer.ClearPlatformTextureSettings("Android");
                importer.SetPlatformTextureSettings(def);
                EditorUtility.SetDirty(importer);
                AssetDatabase.WriteImportSettingsIfDirty(path);
            }
        }

        public static void SetPackingTag(string path, string tag)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                if (!importer.spritePackingTag.Equals(tag))
                {
                    importer.spritePackingTag = tag;
                    EditorUtility.SetDirty(importer);
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                }
            }
        }
        
        public static void ConvertTextureToHsv(Texture2D texture)
        {
                var path = AssetDatabase.GetAssetPath(texture);
                var newTexture = new Texture2D(texture.width, texture.height);

                var rgbs = texture.GetPixels();
                var hsvs = new Color[rgbs.Length];

                for (var i = 0; i < rgbs.Length; i++)
                {
                    float h, s, v;
                    Color.RGBToHSV(rgbs[i], out h, out s, out v);
                    hsvs[i].r = h;
                    hsvs[i].g = s;
                    hsvs[i].b = v;
                    hsvs[i].a = rgbs[i].a;
                }

                newTexture.SetPixels(hsvs);
                var bytes = newTexture.EncodeToPNG();
                File.WriteAllBytes(path.Replace(".", "_hsv."), bytes);
                AssetDatabase.Refresh();
        }
    }
}