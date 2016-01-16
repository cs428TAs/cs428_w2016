using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TilesetWizard : ScriptableWizard
{
    public delegate void OnCreate(Tileset tileset);

    private string _path;
    private string _name;
    public string AssetPath;
    public string Name;
    public int TileSize = 64;
    public int Collision = 1; 

    private bool _setup;

    private OnCreate _onCreate;

    public void SetFile(string s, OnCreate onCreate)
    {
        _path = s;
        AssetPath = _path;
        _name = Path.GetFileNameWithoutExtension(_path);

        _onCreate = onCreate;

        OnWizardUpdate();
    }

    public void OnWizardCreate()
    {
        if (string.IsNullOrEmpty(_path))
        {
            Close();
        }

        var tileSet = CreateInstance<Tileset>();
        tileSet.Initialize();
        tileSet.name = _name;
        tileSet.TileSize = TileSize;
        tileSet.Collision = Collision;

        CreateTileset(tileSet);

        _onCreate(tileSet);
    }

    public void OnWizardUpdate()
    {
        helpString = "Setup for new Tileset.";

        AssetPath = _path;
        Name = _name;

        if (string.IsNullOrEmpty(AssetPath))
        {
            errorString = "Error. Please close and load again.";
            isValid = false;
        }
        else if (!IsPowerOfTwo(TileSize))
        {
            errorString = "Tilesize must be power of 2";
            isValid = false;
        }
        else if (TileSize < 16 || TileSize > 1024)
        {
            errorString = "Tilesize must be between 16 and 1024";
            isValid = false;
        }
        else if (Collision < 1)
        {
            errorString = "Collision must be between greater than 0";
            isValid = false;
        }
        else
        {
            errorString = "";
            isValid = true;
        }
    }

    public void CreateTileset(Tileset t)
    {
        AssetDatabase.DeleteAsset(_path);
        AssetDatabase.CreateAsset(t, _path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private bool IsPowerOfTwo(int x)
    {
        return (x & (x - 1)) == 0;
    }
}