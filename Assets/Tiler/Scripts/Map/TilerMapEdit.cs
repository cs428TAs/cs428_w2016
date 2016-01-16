using System;
using System.Collections.Generic;
using Tiler;
using UnityEngine;

public class TilerMapEdit
{
    private readonly TilerMap _tilerMap;
    public UndoPaint Undo;

    public TilerMapEdit(TilerMap tilerMap)
    {
        _tilerMap = tilerMap;
        Undo = new UndoPaint(this);
    }

    public void PaintTile(Point tileID, IBrush brush)
    {
        if (brush == null)
        {
            Debug.Log("Array doesn't exist");
            return;
        }

        var applyList = new HashSet<Texture2D>();

        // How far we travel
        var right = brush.BrushSize.X / 2;
        var left = (brush.BrushSize.X - 1) / 2;
        var up = brush.BrushSize.Y / 2;
        var down = (brush.BrushSize.Y - 1) / 2;

        for (var y = -down; y <= up; y++)
        {
            for (var x = -left; x <= right; x++)
            {
                var tid = new Point(tileID.X + x, tileID.Y + y);

                var data = brush.GetBrush(x, y);

                var changedTexture = ChangeTile(tid, data);
                if (changedTexture != null)
                    applyList.Add(changedTexture);
            }
        }

        // Apply any changes
        foreach (var texture in applyList)
        {
            texture.Apply();
        }
    }

    public void FillTiles(Point startPoint, NormalBrush brush)
    {
        var applyList = new HashSet<Texture2D>();

        var queue = new Queue<Point>();
        queue.Enqueue(startPoint);

        var targetTile = _tilerMap.GetTileFromWorldTile(startPoint, true);//.Properties.ID;
        if (targetTile == null) return;

        var targetProperty = targetTile.Properties;
        var replacementID = brush.GetBrush(startPoint.X, startPoint.Y);

        // Already match, just return
        if (replacementID.Properties.Equals(targetProperty))
            return;

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            var tile = _tilerMap.GetTileFromWorldTile(p);

            if (tile != null && tile.Properties.Equals(targetProperty))
            {
                var changedTexture = ChangeTile(p, replacementID);
                applyList.Add(changedTexture);

                var left = new Point(p.X - 1, p.Y);
                queue.Enqueue(left);

                var right = new Point(p.X + 1, p.Y);
                queue.Enqueue(right);

                var bottom = new Point(p.X, p.Y - 1);
                queue.Enqueue(bottom);

                var top = new Point(p.X, p.Y + 1);
                queue.Enqueue(top);
            }
        }

        // Apply any changes
        foreach (var t in applyList)
        {
            t.Apply();
        }
    }

    public void ReplaceTiles(Point startPoint, NormalBrush brush)
    {
        var applyList = new HashSet<Texture2D>();

        var targetTile = _tilerMap.GetTileFromWorldTile(startPoint, true);//.Properties.ID;
        if (targetTile == null) return;

        var targetProperty = targetTile.Properties;
        var replacementID = brush.GetBrush(startPoint.X, startPoint.Y);

        // Already match, just return
        if (replacementID.Properties.Equals(targetProperty))
            return;

        foreach (var cell in _tilerMap.Cells)
        {
            for (int index = 0; index < cell.Tiles.Length; index++)
            {
                var tile = cell.Tiles[index];

                if (tile.Properties.Equals(targetProperty))
                {
                    var p = new Point
                                {
                                    X = index%_tilerMap.TilesPerCell, 
                                    Y = index/_tilerMap.TilesPerCell
                                };

                    var changedTexture = ChangeTile(cell, p, replacementID);
                    applyList.Add(changedTexture);
                }
            }
        }

        // Apply any changes
        foreach (var t in applyList)
        {
            t.Apply();
        }
    }

    public Texture2D ChangeTile(Point worldPoint, Brush data)
    {
        var cell = _tilerMap.FindOrCreateCell(worldPoint);
        var cellID = _tilerMap.GetCellIDFromWorldTileID(worldPoint);

        var normPoint = worldPoint - (cellID * _tilerMap.TilesPerCell);
        normPoint += (_tilerMap.TilesPerCell / 2 - 1);

        return ChangeTile(cell, normPoint, data);
    }

    public Texture2D ChangeTile(Cell cell, Point localTileID, Brush data)
    {
        var index = localTileID.Y*_tilerMap.TilesPerCell + localTileID.X;
        var t = cell.GetTile(index);

        if (t.Properties != data.Properties)
        {
            var oldBrush = _tilerMap.GetTileBrush(cell, localTileID);
            Undo.PushUndo(cell, localTileID, oldBrush);

            t.Properties = new TileProperties(data.Properties);

            var texture = cell.GetTexture();
            if (!texture)
                return null;

            texture.SetPixels(localTileID.X*_tilerMap.TileResolution, localTileID.Y*_tilerMap.TileResolution, _tilerMap.TileResolution,
                              _tilerMap.TileResolution, data.Colors);

            var c = data.Collision;
            var collision = new bool[c.Length];
            Array.Copy(c, collision, c.Length);

            t.Collision = collision;

            // Just add to apply list so we don't run multiple applies for same texture
            cell.IsDirty = true;

            return texture;
        }

        return null;
    }
}
