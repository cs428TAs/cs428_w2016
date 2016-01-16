using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tiler.Editor
{
    public static class EditorUtil
    {
        public static void SetTextureImportSettings(string path, int textureResolution)
        {
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                AssetDatabase.StartAssetEditing();
                textureImporter.textureType = TextureImporterType.Advanced;
                textureImporter.isReadable = true;
                textureImporter.maxTextureSize = textureResolution;
                textureImporter.mipmapEnabled = true; // turn these off for now otherwise drawing is extremely slow
                textureImporter.textureFormat = TextureImporterFormat.ARGB32;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
                AssetDatabase.StopAssetEditing();
            }
        }
        public static void SetTextureReadable(Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);

            if (File.Exists(path))
            {
                var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

                if (textureImporter != null && !textureImporter.isReadable)
                {
                    AssetDatabase.StartAssetEditing();
                    textureImporter.textureType = TextureImporterType.Advanced;
                    textureImporter.isReadable = true;
                    textureImporter.textureFormat = TextureImporterFormat.ARGB32;
                    AssetDatabase.ImportAsset(path);
                    AssetDatabase.Refresh();
                    AssetDatabase.StopAssetEditing();
                }
            }
        }
    }
}
