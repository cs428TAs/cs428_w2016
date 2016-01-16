using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tiler;
using UnityEngine;

[Serializable]
public class Tileset : ScriptableObject
{
    public const int CurrentVersion = 1;
    public int Version = 0;

    public List<TileTexture> Assets;
    public int TileSize = 64;
    public int Collision = 1;

    public int TilesetID;

    [SerializeField]private int _tileID;
    private int NextTileID
    {
        get
        {
            return ++_tileID;
        }
    }

    public List<TileTexture> GetAssets()
    {
        return Assets.ToList();
    }

    public void Initialize()
    {
        TilesetID = GetInstanceID();
        Assets = new List<TileTexture>();
        Version = CurrentVersion;
    }

    public TileTexture AddAsset(Texture2D texture)
    {
        TileTexture exists = Assets.FirstOrDefault(t => t.Texture == texture);

        if (exists == null)
        {
            int id = NextTileID;

            long uid = ((long)TilesetID << 32) | (uint)id;
            texture.name = id.ToString(CultureInfo.InvariantCulture);
            var tt = new TileTexture(uid, texture, Collision);
            Assets.Add(tt);

            return tt;
        }

        return exists;
    }

    public void RemoveAsset(Texture2D texture)
    {
        var asset = Assets.Find(t => t.Texture == texture);
        Assets.Remove(asset);
    }

    public TileTexture GetAssetFromID(int id)
    {
        if (id == 0)
            return TileTexture.None;

        return Assets.FirstOrDefault(t => t.ID == id);
    }

#if UNITY_EDITOR
    public void VersionCheck()
    {
        // Version 0 needs tileID fixes
        if (Version < 1)
        {
            // A problem in 1.0 caused tileID not to save, this fix resets the tileValue for any Tileset created before then
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                var fullPath = Application.dataPath;
                var strippedPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "/";
                var index = fullPath.IndexOf("Assets", StringComparison.Ordinal);
                if (index == -1)
                    return;

                fullPath = fullPath.Substring(0, index);
                fullPath += strippedPath;

                var files = Directory.GetFiles(fullPath, "*.png");
                
                int max = 0;
                foreach (var f in files)
                {
                    var s = Path.GetFileNameWithoutExtension(f);
                    int i;

                    if (int.TryParse(s, out i))
                    {
                        if (i > max)
                            max = i;
                    }
                }

                _tileID = max;
            }
        }

        Version = CurrentVersion;
    }
#endif
}