using System;
using System.IO;
using Tiler;
using UnityEditor;
using UnityEngine;

[Serializable]
public sealed class TilesetWindow : Section
{
    private const int ListWidth = 250;
    private const int PropertyWidth  = 450;
    private const int AddHeight = 200;

    private TilesetListWindow _tsWindow;
    private TilesetAddWindow _addWindow;
    private TilesetPropertiesWindow _propertyWindow;

    private Texture2D _newTileTexture;

    public Tileset Tileset;

    public TileTexture CurrentSelection;
    private int _currentIndex;
    private bool _delete;

    // GUI Elements
    private Vector2 _textureScroll = Vector2.zero;

    public TilesetWindow(TilerWindow window)
        : base(window)
    {
        OnEnable();
    }

    public override void New()
    {
        var fullPath = Application.dataPath + TilerWindow.DataPath + "Tilesets/";

        // Check if directory exists, if not create it
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        var path = EditorUtility.SaveFilePanel("New Tileset", fullPath,
                                               "tileset", "asset");
        if (String.IsNullOrEmpty(path))
            return;

        var index = path.IndexOf("Assets", StringComparison.Ordinal);
        if (index == -1)
        {
            Debug.Log("Path must be inside project asset directory");
            return;
        }

        var unityPath = path.Substring(index);

        var tw = ScriptableWizard.DisplayWizard<TilesetWizard>("Create Tileset", "Create");
        tw.SetFile(unityPath, OnNew);
    }

    private void OnNew(Tileset tileset)
    {
        var instanceID = tileset.GetInstanceID();
        _tsWindow.SetTileSet(instanceID);
    }

    public override void Save()
    {
        if (Tileset != null)
        {
            EditorUtility.SetDirty(Tileset);
            AssetDatabase.SaveAssets();
        }
    }

    public override void OnGUI()
    {
        var rect = new Rect();
        rect.width = Parent.position.width - PropertyWidth - ListWidth;
        rect.height = Parent.position.height;

        GUILayout.BeginArea(rect, WindowNoTitleStyle);
        base.OnGUI();

        if (Tileset != null)
        {
            _textureScroll = GUILayout.BeginScrollView(_textureScroll, false, true);
            DrawTextures();
            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();

        Windows();
    }

    private void Windows()
    {
        var windowRect = Parent.position;
        windowRect.x = windowRect.width - ListWidth;
        windowRect.width = ListWidth;
        windowRect.y = 0;

        GUILayout.BeginArea(windowRect, "Tilesets", WindowTitleStyle);
        _tsWindow.OnGUI();
        GUILayout.EndArea();

        windowRect.x -= PropertyWidth;
        windowRect.width = PropertyWidth;
        windowRect.y = 0;
        windowRect.height = Parent.position.height - AddHeight;

        GUILayout.BeginArea(windowRect, "Tile Properties", WindowTitleStyle);
        _propertyWindow.OnGUI();
        GUILayout.EndArea();

        windowRect.y = windowRect.height;
        windowRect.height = AddHeight;

        GUILayout.BeginArea(windowRect, "Add Tiles", WindowTitleStyle);
        _addWindow.OnGUI();
        GUILayout.EndArea();
    }

    public override void Close()
    {
        base.Close();

        Destroy();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        _tsWindow = new TilesetListWindow(this);
        _addWindow = new TilesetAddWindow(this);
        _propertyWindow = new TilesetPropertiesWindow(this);

        UnityInternal.AddGlobalEventHandler(HandleInput);
    }

    public override void OnDisable(bool compiling)
    {
        base.OnDisable(compiling);

        _tsWindow.OnDisable();
        _propertyWindow.OnDisable();

        UnityInternal.RemoveGlobalEventHandler(HandleInput);
    }

    private void HandleInput()
    {
        if (Event.current.type == EventType.keyDown)
        {
            if (Event.current.keyCode == KeyCode.A)
            {
                _currentIndex = Math.Max(_currentIndex - 1, 0);
                _currentIndex = Math.Min(_currentIndex, Tileset.Assets.Count - 1);

                SetTile(_currentIndex);
            }
            if (Event.current.keyCode == KeyCode.D)
            {
                _currentIndex = Math.Min(_currentIndex + 1, Tileset.Assets.Count - 1);

                SetTile(_currentIndex);
            }
            if (Event.current.keyCode == KeyCode.Delete)
            {
                if (Delete())
                {
                    CurrentSelection = null;
                    _currentIndex = -1;
                    return;
                }
            }
            if (Event.current.keyCode == KeyCode.Alpha1)
            {
                CurrentSelection.Connections ^= (ConnectionMask)(1 << 0);
            }
            if (Event.current.keyCode == KeyCode.Alpha2)
            {
                CurrentSelection.Connections ^= (ConnectionMask)(1 << 1);
            }
            if (Event.current.keyCode == KeyCode.Alpha3)
            {
                CurrentSelection.Connections ^= (ConnectionMask)(1 << 2);
            }
            if (Event.current.keyCode == KeyCode.Alpha4)
            {
                CurrentSelection.Connections ^= (ConnectionMask)(1 << 3);
            }
            if (Event.current.keyCode == KeyCode.Alpha5)
            {
                //all
                CurrentSelection.Connections = (ConnectionMask)15;
            }
            if (Event.current.keyCode == KeyCode.Alpha6)
            {
                CurrentSelection.Connections = 0;
            }

            Repaint();
        }
    }

    private void DrawTextures()
    {
        const int space = 2;
        const int previewSize = 64;

        // 15 for scrollbar
        var size = Parent.position.width - PropertyWidth - ListWidth - WindowPadding * 2;

        var width = (int)Mathf.Max((size - 16) / (previewSize + space), 1);

        for (int index = 0; index < Tileset.Assets.Count; index += width)
        {
            EditorGUILayout.BeginHorizontal();

            var hor = Mathf.Min(Tileset.Assets.Count - index, width);

            for (var i = 0; i < hor; i++)
            {
                var assetIndex = index + i;
                var t = Tileset.Assets[assetIndex];

                var rect = GUILayoutUtility.GetRect(64, 64, GUIStyle.none);
                GUI.color = CurrentSelection == t ? new Color(153 / 255f, 204 / 255f, 1f, 1) : Color.white;

                GUI.DrawTexture(rect, t.Texture);

                if (Event.current.type == EventType.MouseDown)
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.button == 0)
                        {
                            SetTile(assetIndex);
                        }
                    }
                }
                
                rect.x += rect.width - 20;
                rect.width = 20;
                rect.height = 20;

                GUI.color = Color.red;
                if (GUI.Button(rect, "X"))
                {
                    if (Delete())
                    {
                        i--;
                        hor = Mathf.Min(Tileset.Assets.Count - index, width);

                        if (t == CurrentSelection)
                        {
                            CurrentSelection = null;
                            _currentIndex = -1;
                        }
                    }
                }

                GUILayout.Space(space);
                GUI.color = Color.white;
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(space);
        }
    }

    private void SetTile(int index)
    {
        if (index == -1) return;

        CurrentSelection = Tileset.Assets[index];
        _currentIndex = index;
        Repaint();
    }

    public void SetTileset(Tileset ts)
    {
        Tileset = ts;
        CurrentSelection = null;
        _currentIndex = -1;
        Repaint();

        var nullAssets = Tileset.Assets.FindAll(t => t.Texture == null);

        foreach (var asset in nullAssets)
        {
            Tileset.Assets.Remove(asset);
        }
    }

    private bool Delete()
    {
        if (CurrentSelection == null) return false;

        if (EditorUtility.DisplayDialog("Delete texture from set?",
                                        "Are you sure you want to delete texture?",
                                        "Delete", "Cancel"))
        {
            Tileset.Assets.Remove(CurrentSelection);

            var p = AssetDatabase.GetAssetPath(CurrentSelection.Texture);
            AssetDatabase.DeleteAsset(p);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Repaint();

            Parent.Focus();

            return true;
        }

        return false;
    }
}