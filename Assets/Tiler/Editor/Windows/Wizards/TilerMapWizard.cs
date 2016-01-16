using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TilerMapWizard : EditorWindow
{
    public delegate void OnCreateCallback(Object obj);

    private string _path = string.Empty;

    public string Name = "Map";
    public float TileSize = 1;
    public int TilesPerCell = 16;
    public int TextureResolution = 1024;
    public string DefaultShader = "Diffuse";

    private bool _isValid;
    private string _helpString = "";
    private string _errorString = "";

    private float _height = 0 - 0.000001f;

    private Vector2 _scroll;

    private OnCreateCallback _onCreateCallback;

    public void Set(string path, OnCreateCallback callback)
    {
        _path = path;
        Name = Path.GetFileNameWithoutExtension(path);
        _onCreateCallback = callback;

        var maps = FindObjectsOfType(typeof(TilerMap)).Cast<TilerMap>().ToArray();

        foreach (var map in maps)
        {
            var y = map.transform.position.y;

            if (y > _height)
            {
                _height = y;
            }
        }

        _height += 0.000001f;
    }

    public void OnGUI()
    {
        GUILayout.Label(_helpString);

        _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false);
        EditorGUILayout.LabelField("Name:", Name);

        TileSize = EditorGUILayout.FloatField("Tile Size", TileSize);
        TilesPerCell = EditorGUILayout.IntField("Tiles Per Cell", TilesPerCell);

        var cellSize = TileSize * TilesPerCell;
        EditorGUILayout.LabelField("Size Of Cell", cellSize.ToString(CultureInfo.InvariantCulture));
        
        TextureResolution = EditorGUILayout.IntField("Texture Resolution", TextureResolution);
        var tileResolution = TextureResolution/TilesPerCell;
        EditorGUILayout.LabelField("Tile Resolution", tileResolution.ToString(CultureInfo.InvariantCulture));

        DefaultShader = EditorGUILayout.TextField("Default Shader", DefaultShader);

        EditorGUILayout.EndScrollView();

        GUILayout.Label(_errorString);

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (!_isValid) GUI.enabled = false;
        if (GUILayout.Button("Create"))
        {
            OnCreate();
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }

    public void OnInspectorUpdate()
    {
        _helpString = "Creates a new map layer.";

        if (Name == string.Empty)
        {
            _errorString = "Please assign a name";
            _isValid = false;
        }
        else if (TextureResolution > 4096 || TextureResolution < 128)
        {
            _errorString = "Texture Resolution must be beteween 128 and 4096";
            _isValid = false;
        }
        else if (!IsPowerOf2(TextureResolution))
        {
            _errorString = "Texture Resolution must be a power of 2";
            _isValid = false;
        }
        else if (TilesPerCell > 32)
        {
            _errorString = "Tiles Per Cell must be 32 or less";
            _isValid = false;
        }
        else if (!IsPowerOf2(TilesPerCell))
        {
            _errorString = "Tiles Per Cell must be a power of 2";
            _isValid = false;
        }
        else if (TileSize <= 0)
        {
            _errorString = "Tile Size must be greater than 0";
            _isValid = false;
        }
        else if (Shader.Find(DefaultShader) == null)
        {
            _errorString = "Shader doesn't exist";
            _isValid = false;
        }
        else
        {
            _errorString = "";
            _isValid = true;
        }

        Repaint();
    }

    public void OnCreate()
    {
        var go = TilerMap.Create(Name, TileSize, TilesPerCell, TextureResolution, DefaultShader);
        var pos = go.transform.position;
        pos.y = _height;
        go.transform.position = pos;

        AssetDatabase.DeleteAsset(_path);
        PrefabUtility.CreatePrefab(_path, go, ReplacePrefabOptions.ConnectToPrefab);

        _onCreateCallback(go.GetComponent<TilerMap>());

        Close();
    }

    private bool IsPowerOf2(int x)
    {
        return (x > 0) && ((x & (x - 1)) == 0);
    }
}