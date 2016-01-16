using System;
using System.Collections.Generic;
using System.Linq;
using TileDraw;
using Tiler;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


public class MiscPopup : PopupWindow
{
    private TilerMap _map;

    public void Setup(TilerMap map)
    {
        _map = map;
    }

    public override void OnGUI()
    {
        if (Event.current.type == EventType.mouseMove)
            Repaint();

        if (Menu("Generate NavMesh"))
        {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1
            if (!InternalEditorUtility.HasPro())
            {
                EditorUtility.DisplayDialog("Missing Pro",
                                            "You're version of Unity requires Pro to generate Navigation Meshes.\nAs of Unity 4.2, free users are able to generate Navigation Meshes so upgrade if this is a feature you require.",
                                            "OK");
                Close();
            }
#endif

            GenerateNavMesh();
            Close();
        }

        GUILayout.Space(5);

        if (Menu("Rebuild Textures"))
        {
            if (_map != null &&
                EditorUtility.DisplayDialog("Rebuild Textures", "BETA FUNCTION: Backup before proceeding.\n" +
                                                                "This will rebuild all textures from saved data.\n" +
                                                                "You should only do this if your textures are corrupt and don't match the saved data.\n" +
                                                                "On large maps this function may take several minutes to complete. Please be patient.",
                                            "Rebuild", "Cancel"))
            {
                Rebuild();
                Close();
            }
        }

        base.OnGUI();
    }

    private bool Menu(string optionName)
    {
        var gc = new GUIContent(optionName);

        var rect = GUILayoutUtility.GetRect(gc, LabelStyle);

        if (rect.Contains(Event.current.mousePosition))
        {
            LabelStyle.normal.textColor = Color.red;
        }

        GUI.Label(rect, gc, LabelStyle);
        LabelStyle.normal.textColor = DefaultLabelColor;

        if (MyGUI.ButtonMouseDown(rect))
        {
            return true;
        }
        return false;
    }

    private void Rebuild()
    {
        var tpc = _map.TilesPerCell;

        var tilesets = LoadAllTilesets();

        foreach (Cell cell in _map.Cells)
        {
            var texture = cell.GetTexture();
            if (!texture)
                continue;

            for (var j = 0; j < tpc; j++)
            {
                for (var i = 0; i < tpc; i++)
                {
                    var index = j*tpc + i;
                    var tile = cell.Tiles[index];

                    NormalBrush brush = GetBrush(tilesets, tile, _map.TileResolution);

                    if (brush != null)
                    {
                        var data = brush.GetBrush();

                        texture.SetPixels(i*_map.TileResolution, j*_map.TileResolution, _map.TileResolution,
                                          _map.TileResolution, data.Colors);

                        var c = data.Collision;
                        var collision = new bool[c.Length];
                        Array.Copy(c, collision, c.Length);

                        tile.Collision = collision;
                    }
                }
            }
            texture.Apply();
        }
    }

    private static List<Tileset> LoadAllTilesets()
    {
        var tilesetData = UnityInternal.GetAssetsInProjectOfType<Tileset>().ToArray();
        var tilesets = new List<Tileset>();

// ReSharper disable LoopCanBeConvertedToQuery
        foreach (var t in tilesetData)
// ReSharper restore LoopCanBeConvertedToQuery
        {
            var path = AssetDatabase.GetAssetPath(t.InstanceID);
            var tileset = AssetDatabase.LoadAssetAtPath(path, typeof (Tileset)) as Tileset;
            tilesets.Add(tileset);
        }

        return tilesets;
    }
    private NormalBrush GetBrush(IEnumerable<Tileset> tilesets, Tile tile, int textureSize)
    {
        var c = tile.Collision;
        var collision = new bool[c.Length];
        Array.Copy(c, collision, c.Length);

        var id = tile.Properties.ID;
        var setID = (int) (id >> 32);

        Tileset ts = null;

        // Find matching tileset
// ReSharper disable LoopCanBeConvertedToQuery
        foreach (var t in tilesets)
// ReSharper restore LoopCanBeConvertedToQuery
        {
            if (t.TilesetID == setID)
            {
                ts = t;
                break;
            }
        }

        TileTexture tt = null;

        if (ts)
        {
            // Find matching tile
            tt = ts.Assets.Find(a => a.ID == id);
        }

        if (tt == null)
        {
            tt = TileTexture.None;
        }

        var brush = new NormalBrush(textureSize, tt);

        for (var i = 0; i < 4; i++)
        {
            if (brush.GetBrush().Properties.Rot == tile.Properties.Rot)
                break;

            brush.Rotate();
        }

        return brush;
    }

    private void GenerateNavMesh()
    {
        var maps = FindObjectsOfType(typeof(TilerMap)).Cast<TilerMap>().ToArray();

        foreach (var map in maps)
        {
            var collectionSize = 1;
            foreach (var cell in map.Cells)
            {
                foreach (var t in cell.Tiles)
                {
                    var size = t.Collision.Length;
                    if (size > collectionSize)
                        collectionSize = size;
                }
            }

            collectionSize = (int) Mathf.Sqrt(collectionSize);

            foreach (var cell in map.Cells)
            {
                var tiles = cell.Tiles;

                var goTrans = cell.transform.Find("navmesh");

                if (goTrans != null)
                    DestroyImmediate(goTrans.gameObject);

                goTrans = new GameObject("navmesh").transform;

                goTrans.parent = cell.transform;
                goTrans.localPosition = new Vector3();

                var collection = new bool[map.TilesPerCell,map.TilesPerCell][];

                for (var y = 0; y < map.TilesPerCell; y++)
                {
                    for (var x = 0; x < map.TilesPerCell; x++)
                    {
                        collection[y, x] = tiles[y*map.TilesPerCell + x].Collision;
                    }
                }

                var merged = Util.MergeArrays(collection, collectionSize);

                var c = new Combine();
                var r = c.FindRect(merged);

                var size = r.GetLength(0);
                var sizePerCollider = map.CellSize/size;
                var halfCellSize = map.CellSize/2f;

                var offset = sizePerCollider/2f;

                var p = new Point();

                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var start = r[y, x];
                        if (start != p)
                        {
                            var xx = (x + x + start.X - 1)/2f;
                            var yy = (y + y - start.Y + 1)/2f;

                            var posX = sizePerCollider*xx - (halfCellSize - offset);
                            var posY = sizePerCollider*yy - (halfCellSize - offset);

                            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            go.isStatic = true;
                            go.transform.parent = goTrans;

                            var goPos = new Vector3();
                            goPos.x = posX;
                            goPos.y = 0;
                            goPos.z = posY;

                            go.transform.localPosition = goPos;
                            go.transform.localScale = new Vector3(start.X*sizePerCollider, 1, start.Y*sizePerCollider);

                            GameObjectUtility.SetNavMeshLayer(go, 1);
                        }
                    }
                }
            }
        }

        NavMeshBuilder.ClearAllNavMeshes();
        NavMeshBuilder.BuildNavMesh();

        foreach (var map in maps)
        {
            foreach (var cell in map.Cells)
            {
                var goTrans = cell.transform.Find("navmesh");
                DestroyImmediate(goTrans.gameObject);
            }
        }
    }
}