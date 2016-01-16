using Tiler;
using UnityEditor;
using UnityEngine;

public class TilesetListWindow : IChildWindow
{
    private TilesetWindow _parent;
    public AssetResult[] TilesetList;

    private Vector2 _scroll;

    public TilesetListWindow (TilesetWindow parent)
    {
        _parent = parent;
        LoadTilesets();

        EditorApplication.projectWindowChanged += LoadTilesets;
    }

    private void LoadTilesets()
    {
        TilesetList = UnityInternal.GetAssetsInProjectOfType<Tileset>().ToArray();

        // if no tileset set yet, set
        if (TilesetList.Length > 0 && !_parent.Tileset)
        {
            SetTileSet(TilesetList[0].InstanceID);
        }

        _parent.Repaint();
    }
    public void OnDisable()
    {
        EditorApplication.projectWindowChanged -= LoadTilesets;
    }

    public void OnGUI()
    {
        if (_parent.Tileset == null) return;

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        for (int index = 0; index < TilesetList.Length; index++)
        {
            var ts = TilesetList[index];
            
            var style = ts.InstanceID == _parent.Tileset.GetInstanceID()
                            ? EditorStyles.whiteLargeLabel
                            : EditorStyles.largeLabel;

            if (GUILayout.Button(ts.Name, style))
            {
                // if it fails, it means the tileset doesn't exist anymore so remove it
                SetTileSet(ts.InstanceID);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    public void SetTileSet(int id)
    {
        var path = AssetDatabase.GetAssetPath(id);

        var tileset = AssetDatabase.LoadAssetAtPath(path, typeof (Tileset)) as Tileset;

        _parent.SetTileset(tileset);
    }
}
