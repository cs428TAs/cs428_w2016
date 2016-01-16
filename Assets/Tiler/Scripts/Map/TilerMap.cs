using System;
using System.Collections.Generic;
using TileDraw;
using Tiler;
using UnityEngine;

[Serializable]
public class TilerMap : MonoBehaviour
{
    [SerializeField] private List<Cell> _cells;
    [SerializeField] private float _tileSize;
    [SerializeField] private int _tilesPerCell;
    [SerializeField] private int _textureResolution;
    [SerializeField] private string _defaultShader;
    [SerializeField] private Mesh _sharedMesh;
    [SerializeField] private int _layer;

    public float TileSize
    {
        get { return _tileSize; }
    }
    public int TilesPerCell
    {
        get { return _tilesPerCell; }
    }
    public float CellSize
    {
        get { return _tileSize*_tilesPerCell; }
    }
    public int TextureResolution
    {
        get { return _textureResolution; }
    }
    public int TileResolution
    {
        get { return _textureResolution/TilesPerCell; }
    }
    public string DefaultShader
    {
        get { return _defaultShader; }
    }

    public List<Cell> Cells
    {
        get { return _cells; }
    }

    public Mesh SharedMesh
    {
        get { return _sharedMesh; }
        set { _sharedMesh = value; }
    }

    public int Layer
    {
        get { return _layer; }
        set { _layer = value; }
    }

    public static GameObject Create(string name, float tileSize, int tilesPerCell, int textureResolution, string defaultShader)
    {
        var go = new GameObject(name);

        var mm = go.AddComponent<TilerMap>();
        mm.Setup(tileSize, tilesPerCell, textureResolution, defaultShader);

        return go;
    }

    public Cell FindCell(Point cellID)
    {
        var cellName = Cell.GetName(cellID);

        for (var i = 0; i < _cells.Count; i++)
        {
            var c = _cells[i];
            if (c == null)
            {
                _cells.Remove(c);
                i--;
                continue;
            }

            if (c.name == cellName)
                return c;
        }

        return null;
        //return _cells.FirstOrDefault(c => c.name == cellName);
    }
    public Cell FindCellFromTile(Point tileID)
    {
        var cellID = GetCellIDFromWorldTileID(tileID);
        var cell = FindCell(cellID);

        return cell;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tileID">Cell id</param>
    /// <returns></returns>
    public Cell FindOrCreateCell(Point tileID)
    {
        var cellID = GetCellIDFromWorldTileID(tileID);
        var cell = FindCell(cellID);

        if (cell == null)
            cell = CreateCell(cellID);

        return cell;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="world">Point in world</param>
    /// <returns></returns>
    public Cell FindOrCreateCell(Vector3 world)
    {
        var tile = GetWorldTileIDFromWorldCoords(world);
        return FindOrCreateCell(tile);
    }

    public Point GetCellIDFromWorldTileID(Point world)
    {
        var vec2 = new Vector2();

        // offset by half a cell
        vec2.x = world.X - 0.5f;
        vec2.y = world.Y - 0.5f;

        vec2.x /= TilesPerCell;
        vec2.y /= TilesPerCell;

        vec2.x = Util.RoundTo(vec2.x, 1);
        vec2.y = Util.RoundTo(vec2.y, 1);

        return new Point((int)vec2.x, (int)vec2.y);
    }

    public Point GetWorldTileIDFromWorldCoords(Vector3 world)
    {
        world -= transform.position;

        var vec2 = new Vector2(Util.RoundTo(world.x, TileSize) / TileSize, Util.RoundTo(world.z, TileSize) / TileSize);
        return new Point((int)vec2.x, (int)vec2.y);
    }
    public Point GetLocalTileFromWorldTile(Point tileID)
    {
        var cellID = GetCellIDFromWorldTileID(tileID);

        var normPoint = tileID - (cellID * TilesPerCell);

        normPoint += (TilesPerCell / 2 - 1);

        return normPoint;
    }

    public Tile GetTileFromWorldTile(Point tileID, bool create = false)
    {
        var cell = create ? FindOrCreateCell(tileID) : FindCellFromTile(tileID);
        //var cell = 
        if (cell == null) return null;

        var local = GetLocalTileFromWorldTile(tileID);
        var index = local.Y*TilesPerCell + local.X;

        return cell.GetTile(index);
    }

    public Brush GetTileBrush(Point tileID)
    {
        var cell = FindOrCreateCell(tileID);
        var localTile = GetLocalTileFromWorldTile(tileID);

        return GetTileBrush(cell, localTile);
    }

    public Brush GetTileBrush(Cell cell, Point localTileID)
    {
        var texture = cell.GetTexture();
        if (!texture) throw new UnityException("Texture is missing");

        var index = localTileID.Y*TilesPerCell + localTileID.X;
        var tile = cell.GetTile(index);

        var colors = texture.GetPixels(localTileID.X*TileResolution, localTileID.Y*TileResolution, TileResolution,
                                       TileResolution);

        var c = tile.Collision;
        var collision = new bool[c.Length];
        Array.Copy(c, collision, c.Length);

        return new Brush(colors, new TileProperties(tile.Properties), collision);
    }

    public void SetShader(Shader s)
    {
        foreach (var c in Cells)
        {
            if (c != null)
            {
                c.GetComponent<Renderer>().sharedMaterial.shader = s;
            }
        }

        _defaultShader = s.name;
    }

    private void Setup(float tileSize, int tilesPerCell, int textureResolution, string defaultShader)
    {
        _cells = new List<Cell>();
        _tileSize = tileSize;
        _tilesPerCell = tilesPerCell;
        _textureResolution = textureResolution;
        _defaultShader = defaultShader;
    }

    private Cell CreateCell(Point p)
    {
        var cell = Cell.Create(p, this);
        _cells.Add(cell);
        return cell;
    }
}