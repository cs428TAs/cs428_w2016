using System;
using TileDraw;
using Tiler;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class TilesetPropertiesWindow : IChildWindow
{
    private const int OverlaySize = 16;

    private readonly TilesetWindow _parent;
    private readonly Texture2D _overlay;
    private readonly Texture2D _highlight1;
    private readonly Texture2D _highlight2;

    private Point _lastPoint = new Point(-1,-1);
    private bool _setType;

    public TilesetPropertiesWindow (TilesetWindow parent)
    {
        _parent = parent;

        _overlay = new Texture2D(OverlaySize, OverlaySize, TextureFormat.ARGB32, false);
        _highlight1 = new Texture2D(OverlaySize, OverlaySize, TextureFormat.ARGB32, false);
        _highlight2 = new Texture2D(OverlaySize, OverlaySize, TextureFormat.ARGB32, false);

        var colorO = new Color32(255, 255, 0, 127);
        var colorsO = Util.InitilizeArray(OverlaySize, colorO);

        _overlay.SetPixels32(colorsO);
        _overlay.Apply();

        var colorH1 = new Color32(255, 0, 255, 127);
        var colorsH1 = Util.InitilizeArray(OverlaySize, colorH1);

        _highlight1.SetPixels32(colorsH1);
        _highlight1.Apply();

        var colorH2 = new Color32(0, 255, 255, 127);
        var colorsH2 = Util.InitilizeArray(OverlaySize, colorH2);

        _highlight2.SetPixels32(colorsH2);
        _highlight2.Apply();
    }

    public void OnDisable()
    {
        Object.DestroyImmediate(_overlay);
        Object.DestroyImmediate(_highlight1);
        Object.DestroyImmediate(_highlight2);
    }

  public void OnGUI()
    {
        if (_parent == null) return;

        var e = Event.current;

        if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
            _parent.Repaint();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);

        GUILayout.BeginVertical();

        var selection = _parent.CurrentSelection;
        var tileset = _parent.Tileset;

        if (e.type != EventType.Ignore)
        if (selection != null && selection.Texture != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var scaledSize = Mathf.Max(tileset.TileSize, tileset.Collision * 20);
            var size = Mathf.Min(256, scaledSize);
            //const int size = 256;
            var rect = GUILayoutUtility.GetRect(size, size);
            GUI.DrawTexture(rect, selection.Texture);

            var mouse = Event.current.mousePosition;

            var points = _parent.Tileset.Collision;
            var cMatrix = new bool[selection.Collision.Length];

            Array.Copy(selection.Collision, cMatrix, selection.Collision.Length);

            int index = -1;

            if (rect.Contains(mouse))
            {
                // Convert mouse coords to collision point
                var offset = new Vector2
                                 {
                                     x = mouse.x - rect.x, 
                                     y = mouse.y - rect.y
                                 };

                offset /= rect.width;
                offset.y = 1 - offset.y;

                offset *= points;
                var p = new Point
                            {
                                X = Mathf.Min((int) offset.x, points - 1), 
                                Y = Mathf.Min((int) offset.y, points - 1)
                            };

                index = p.Y * points + p.X;

                
                if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && (p != _lastPoint))
                {    
                     if(e.type == EventType.MouseDown)
                         _setType = !selection.Collision[index];

                    selection.Collision[index] = _setType;
                    _parent.Repaint();
                    _lastPoint = p;
                }
            }

            if (e.type == EventType.MouseUp)
            {
                _lastPoint = new Point(-1, -1);
            }

            for (var y = 0; y < points; y++)
            {
                for (var x = 0; x < points; x++)
                {
                    var i = y * points + x;
                    var pDraw = new Point(x, y);
                    var overlayRect = PointToRect(rect, pDraw);

                    if (cMatrix[i])
                    {
                        GUI.DrawTexture(overlayRect, i == index ? _highlight1 : _overlay);
                    }
                    else
                    {
                        if (i == index)
                        {
                            GUI.DrawTexture(overlayRect, _highlight2);
                        }
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var id = (int)selection.ID;

            EditorGUILayout.SelectableLabel("ID: " + id);
            /*var texture = (Texture2D)EditorGUILayout.ObjectField("Texture", selection.Texture, typeof(Texture2D), false);

            if (texture != selection.Texture)
            {
                var newTexture = new Texture2D(tileset.TileSize, tileset.TileSize);
                var newColor = texture.GetPixels();
                var color = Util.ResizeArray(newColor, tileset.TileSize, texture.width);

                newTexture.SetPixels(color);
                newTexture.Apply();
                newTexture.name = selection.Texture.name;

                DestroyImmediate(selection.Texture, true);
                AssetDatabase.AddObjectToAsset(newTexture, tileset);
                selection.Texture = newTexture;

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }*/

            GUILayout.BeginHorizontal();
            GUILayout.Label("Connections");

            var c = selection.Connections;
            var d = new bool[4];

            d[0] = GUILayout.Toggle((c & ConnectionMask.Left) == ConnectionMask.Left, "Left");
            d[1] = GUILayout.Toggle((c & ConnectionMask.Top) == ConnectionMask.Top, "Top");
            d[2] = GUILayout.Toggle((c & ConnectionMask.Right) == ConnectionMask.Right, "Right");
            d[3] = GUILayout.Toggle((c & ConnectionMask.Bottom) == ConnectionMask.Bottom, "Bottom");

            var all = d[0] && d[1] && d[2] && d[3];
            if (GUILayout.Toggle(all, "All"))
                for (var i = 0; i < 4; i++)
                    d[i] = true;

            var none = !d[0] && !d[1] && !d[2] && !d[3];
            if (GUILayout.Toggle(none, "None"))
                for (var i = 0; i < 4; i++)
                    d[i] = false;

            ConnectionMask connections = 0;
            for (var i = 0; i < 4; i++)
            {
                connections |= (ConnectionMask)((d[i] ? 1 : 0) << i);
            }
            selection.Connections = connections;

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        GUILayout.Space(20);
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    private Rect PointToRect(Rect rect, Point p)
    {
        var points = _parent.Tileset.Collision;
        var width = rect.width / points;
        var height  = rect.height / points;

        rect.x += p.X*width;
        rect.y += (points-1-p.Y) * height;
        rect.width /= points;
        rect.height /= points;

        return rect;
    }
}
