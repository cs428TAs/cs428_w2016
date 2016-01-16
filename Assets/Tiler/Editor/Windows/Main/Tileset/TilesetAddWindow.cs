using System;
using System.Collections.Generic;
using System.IO;
using TileDraw;
using Tiler;
using Tiler.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class TilesetAddWindow : IChildWindow
{
    private TilesetWindow _parent;

    private Texture2D _newTileTexture;
    private int _tilesX = 1;
    private int _tilesY = 1;

    public TilesetAddWindow (TilesetWindow parent)
    {
        _parent = parent;
    }

    public void OnGUI()
    {
        if (_parent == null) return;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);

        GUILayout.BeginVertical();

        var tileset = _parent.Tileset;

        if (tileset != null)
        {
            EditorGUILayout.BeginHorizontal(); // H2
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Texture:");
            _newTileTexture = (Texture2D)EditorGUILayout.ObjectField(_newTileTexture, typeof(Texture2D), false, GUILayout.Width(64),
                                            GUILayout.Height(64));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            _tilesX = EditorGUILayout.IntField("Tiles X", _tilesX, GUILayout.Width(200));
            _tilesX = Mathf.Max(_tilesX, 0);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _tilesY = EditorGUILayout.IntField("Tiles Y", _tilesY, GUILayout.Width(200));
            _tilesY = Mathf.Max(_tilesY, 0);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            if (_newTileTexture == null) GUI.enabled = false;

            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                EditorUtil.SetTextureReadable(_newTileTexture);

                System.Diagnostics.Debug.Assert(_newTileTexture != null, "_newTileTexture.Texture != null");

                var pixelWidth = _newTileTexture.width;
                var pixelsX = (float)_newTileTexture.width / _tilesX;
                var pixelsY = (float)_newTileTexture.height / _tilesY;

                var pixels = _newTileTexture.GetPixels();

                // Loop through each tile
                for (var j = 0; j < _tilesY; j++)
                {
                    var ty0 = (int)(j * pixelsY);
                    var ty1 = (int)((j + 1) * pixelsY);

                    for (var i = 0; i < _tilesX; i++)
                    {
                        var tx0 = (int)(i * pixelsX);
                        var tx1 = (int)((i + 1) * pixelsX);
                        var width = tx1 - tx0;
                        var height = ty1 - ty0;

                        var texture = new Texture2D(tileset.TileSize, tileset.TileSize, TextureFormat.ARGB32, false);
                        var color = new Color[width * height];

                        for (var y = ty0; y < ty1; y++)
                        {
                            var yo = y - ty0;

                            for (var x = tx0; x < tx1; x++)
                            {
                                var xo = x - tx0;

                                color[yo * width + xo] = pixels[y * pixelWidth + x];
                            }
                        }

                        var c = Util.ResizeArray(color, tileset.TileSize, width);
                        texture.SetPixels(c);
                        texture.Apply();

                        AddAsset(texture);
                    }
                }

                _newTileTexture = null;

                _parent.Repaint();
            }

            

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal(); // H2
        }

        GUILayout.EndVertical();

        GUILayout.Space(20);
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    public void AddAsset(Texture2D texture)
    {
        var tsPath = AssetDatabase.GetAssetPath(_parent.Tileset);

        var fullPath = Application.dataPath;
        var strippedPath = Path.GetDirectoryName(tsPath) + "/" + Path.GetFileNameWithoutExtension(tsPath) + "/";

        var index = fullPath.IndexOf("Assets", StringComparison.Ordinal);
        if (index == -1)
            return;

        fullPath = fullPath.Substring(0, index);
        fullPath += strippedPath;

        // Check if directory exists, if not create it
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        var tt = _parent.Tileset.AddAsset(texture);

        // Store data before destroying
        var bytes = texture.EncodeToPNG();
        var texName = texture.name;

        // Clean up old texture - stop memory leaks
        Object.DestroyImmediate(texture, true);

        Util.SaveTextureToFile(bytes, strippedPath + texName + ".png");
        AssetDatabase.Refresh();
        // Load the now saved png 
        var texAsset = (Texture2D)AssetDatabase.LoadAssetAtPath(strippedPath + texName + ".png", typeof(Texture2D));

        // Assign texture importer settings
        var path = AssetDatabase.GetAssetPath(texAsset);
        SetTextureImportSettings(path, _parent.Tileset.TileSize);

        tt.Texture = texAsset;
    }

    public void AssignNewAssets(List<int> ids)
    {
        
    }

    private static void SetTextureImportSettings(string path, int textureResolution)
    {
        var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter != null)
        {
            AssetDatabase.StartAssetEditing();
            textureImporter.textureType = TextureImporterType.Advanced;
            textureImporter.isReadable = true;
            textureImporter.maxTextureSize = textureResolution;
            textureImporter.mipmapEnabled = false;
            textureImporter.textureFormat = TextureImporterFormat.ARGB32;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            AssetDatabase.StopAssetEditing();
        }
    }
}