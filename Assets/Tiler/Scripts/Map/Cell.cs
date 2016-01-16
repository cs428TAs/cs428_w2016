using System;
using TileDraw;
using Tiler;
using UnityEngine;

[Serializable]
public class Cell : MonoBehaviour
{
    [SerializeField]private Tile[] _tiles;

    public bool IsDirty;

    public Tile[] Tiles
    {
        get { return _tiles; }
    }

    public static Cell Create(Point p, TilerMap tilerMap)
    {
        var cellName = GetName(p);

        var go = new GameObject(cellName);
        go.isStatic = true;
        var tr = go.transform;

        tr.parent = tilerMap.transform;

        var position = new Vector3
                           {
                               x = p.X * tilerMap.CellSize + tilerMap.TileSize / 2f,
                               y = 0,
                               z = p.Y * tilerMap.CellSize + tilerMap.TileSize / 2f
                           };

        tr.localPosition = position;

        var mf = go.AddComponent<MeshFilter>();

        if (tilerMap.SharedMesh == null)
            tilerMap.SharedMesh = CreatePlane.Create("_MESH", tilerMap.CellSize, tilerMap.CellSize);
        
        mf.sharedMesh = tilerMap.SharedMesh;

        var mr = go.AddComponent<MeshRenderer>();
        var bc = go.AddComponent<BoxCollider>();
        bc.size = new Vector3(tilerMap.CellSize, 0, tilerMap.CellSize);

        var t = new Texture2D(tilerMap.TextureResolution, tilerMap.TextureResolution, TextureFormat.ARGB32, true);
        var texName = cellName + "_TEX";
        t.name = texName;
        t.wrapMode = TextureWrapMode.Clamp;

        var c = Util.InitilizeArray(tilerMap.TextureResolution, new Color32(205, 205, 205, 0));
        t.SetPixels32(c);
        t.Apply();

        var shader = Shader.Find(tilerMap.DefaultShader);

        var m = new Material(shader);
        var matName = cellName + "_MAT";
        m.name = matName;

        m.mainTexture = t;
        m.renderQueue -= tilerMap.Layer;
        mr.sharedMaterial = m;

        var cell = go.AddComponent<Cell>();
        cell.Setup(tilerMap.TilesPerCell);

        return cell;
    }

    public static string GetName(Point p)
    {
        return p.ToString();
    }

    public Tile GetTile(int index)
    {
        if (index >= _tiles.Length)
        {
            Debug.Log("Index out of range");

            return null;
        }

        return _tiles[index];
    }
    public Texture2D GetTexture()
    {
        if (!GetComponent<Renderer>()) return null;
        
        Texture2D texture;
        if (Application.isPlaying)
        {
            if (!GetComponent<Renderer>().material) return null;
            texture = GetComponent<Renderer>().material.mainTexture as Texture2D;
        }
        else
        {
            if (!GetComponent<Renderer>().sharedMaterial) return null;
            texture = GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
        }

        return texture;
    }

    private void Setup(int tilesPerCell)
    {
        _tiles = new Tile[tilesPerCell*tilesPerCell];
        for (var i = 0; i < _tiles.Length; i++)
        {
            _tiles[i] = new Tile();
        }

        IsDirty = true;
    }
}