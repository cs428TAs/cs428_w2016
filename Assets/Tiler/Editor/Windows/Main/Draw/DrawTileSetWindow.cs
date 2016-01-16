using System;
using System.Collections.Generic;
using System.Linq;
using Tiler.Editor;
using UnityEditor;
using Tiler;
using UnityEngine;

[Serializable]
public class DrawTileSetWindow : IChildWindow
{
    private Tileset _tileset;
    private int _tilesetIndex;
    private List<AssetResult> _tilesetList;
    private string[] _tilesetNames;

    private Tool _beforeTool;
    private Vector2 _scroll;
    private bool _isShowFilter;
    private readonly List<TileTexture> _textures = new List<TileTexture>();
    private readonly Filter _filter = new Filter();

    private DrawWindow _draw;
    private int _currentBrushIndex = -1;

    private readonly float[] _zoomValues = new[] { 0.125f, 0.25f, 0.5f, 0.75f, 1f };
    private readonly string[] _zoomText = new[] { "12%", "25%", "50%", "75%", "100%" };
    private int _zoom = 4; // static so we keep zoom level between sessions

    private Rect windowRect;

    /*public static DrawTileSetWindow Create(DrawWindow draw)
    {
        var window = GetWindow<DrawTileSetWindow>();
        window.title = "Tilesets";
        window._draw = draw;
        return window;
    }*/

    public DrawTileSetWindow(DrawWindow draw)
    {
        _draw = draw;

        OnEnable();
    }

    private void LoadTileSets()
    {
        _tilesetList = UnityInternal.GetAssetsInProjectOfType<Tileset>();
        _tilesetNames = _tilesetList.Select(t => t.Name).ToArray();

        if (_tilesetList.Count > 0)
        {
            SetTileSet(_tilesetList[0].InstanceID);
        }

        UpdateTextureList();
    }

    private void SetTileSet(int id)
    {
        var path = AssetDatabase.GetAssetPath(id);
        _tileset = AssetDatabase.LoadAssetAtPath(path, typeof(Tileset)) as Tileset;
    }

    public void OnGUI()
    {
        if (_draw == null) return;

        //GUI.DragWindow(); 

        Toolbar();

        _scroll = GUILayout.BeginScrollView(_scroll, false, true);
        DrawTiles();
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Zoom");
        _zoom = Mathf.RoundToInt(GUILayout.HorizontalSlider(_zoom, 0, 4, GUILayout.MaxWidth(50)));
        GUILayout.Label(_zoomText[_zoom], GUILayout.MaxWidth(40));
        GUILayout.EndHorizontal();
    }
    public void OnEnable()
    {
        EditorApplication.projectWindowChanged += LoadTileSets;

        _currentBrushIndex = -1;
        LoadTileSets();
    }

    public void OnDisable()
    {
        EditorApplication.projectWindowChanged -= LoadTileSets;
    }

    private void Toolbar()
    {
        GUILayout.BeginHorizontal("toolbar");

        var currentIndex = _tilesetIndex;
        _tilesetIndex = EditorGUILayout.Popup(_tilesetIndex, _tilesetNames, EditorStyles.toolbarPopup);

        if (currentIndex != _tilesetIndex)
        {
            SetTileSet(_tilesetList[_tilesetIndex].InstanceID);
            UpdateTextureList();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.Space();

        if (GUILayout.Button("X", EditorStyles.toolbarButton))
        {
            SetBrush(-1);
        }
        EditorGUILayout.Space();

        GUILayout.Box("Filter", EditorStyles.toolbarDropDown);
        Rect last = GUILayoutUtility.GetLastRect();

        if (MyGUI.ButtonMouseDown(last))
        {
            var win = PopupWindow.ShowAtPosition<FilterPopup>(new Rect(last.x, last.y + last.height, 100f, 150f));
            win.Setup(_filter, UpdateTextureList);
            GUIUtility.ExitGUI();
        }
        else if (MyGUI.ButtonMouseDown(last, 1))
        {
            _filter.Reset();
            UpdateTextureList();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawTiles()
    {
        if (_tileset == null) return;

        const int space = 2;

        var previewSize = (int)(_tileset.TileSize * _zoomValues[_zoom]);

        // 15 for scrollbar
        //GUILayout.Width()
        //var width = Mathf.Max((Screen.width - 16) / (previewSize + space), 1);
        var width = Mathf.Max(198 / (previewSize + space), 1);

        for (int index = 0; index < _textures.Count; index += width)
        {
            EditorGUILayout.BeginHorizontal();

            var hor = Mathf.Min(_textures.Count - index, width);

            for (var i = 0; i < hor; i++)
            {
                var assetIndex = index + i;
                var t = _textures[assetIndex];

                GUI.color = _currentBrushIndex == assetIndex ? new Color(153 / 255f, 204 / 255f, 1f, 1) : Color.white;
                if (GUILayout.Button(t.Texture, GUIStyle.none, GUILayout.Width(previewSize),
                                     GUILayout.Height(previewSize)))
                {
                    SetBrush(assetIndex);
                }
                GUILayout.Space(space);
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(space);
        }
    }

    private void UpdateTextureList()
    {
        if (_tileset == null) return;

        _textures.Clear();

        foreach (var t in _tileset.Assets)
        {
            var rot = _filter.IsFilterWithRotation ? 4 : 1;
            var filter = (int)_filter.ConnectionFilter;

            for (var i = 0; i < rot; i++)
            {
                filter = filter << i;
                if ((filter & 16) == 16) // bx10000
                    filter -= 15; // wrap it back around

                if ((_filter.IsFilterExclusive && (t.Connections == (ConnectionMask)filter)) ||
                    (!_filter.IsFilterExclusive && ((t.Connections & (ConnectionMask)filter) == (ConnectionMask)filter)))
                {
                    _textures.Add(t);
                    break;
                }
            }
        }
    }

    public void SetBrush(int index)
    {
        _currentBrushIndex = index;

        if (index != -1)
        {
            var texture = _textures[index];
            if (!_draw.SetBrush(texture))
            {
                _currentBrushIndex = -1;
            }
        }
        else
        {
            _draw.SetBrush(TileTexture.None);
        }

        _draw.Repaint();
    }
}