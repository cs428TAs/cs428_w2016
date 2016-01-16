using System;
using Tiler;
using UnityEditor;

[Serializable]
public class TilerWindow : EditorWindow
{
    public const int TilerVersion = 1;

    public const string DataPath = @"/Tiler/Data/";

    private bool _isCompiling;
    private Section _section;
    private bool _isClosing;

    public static TilerWindow Create()
    {
        var window = GetWindow<TilerWindow>(new[] {typeof (SceneView)});
        window.Setup();
        return window;
    }

    private void Setup()
    {
        //position = new Rect(50,50,1003,720);

        UnityUpdate.PerformCheck();

        if (_section != null)
            _section.Destroy();

        SetSection(new DrawWindow(this));

        title = "Tiler";
    }

    public void Update()
    {
        if (_isClosing)
            Close();

        if (_section == null)
        {
            _isCompiling = false;

            var typeName = EditorPrefs.GetString("TilerLastWindow", typeof (DrawWindow).ToString());
            var type = Type.GetType(typeName);
            if (type != null)
            {
                var section = (Section) Activator.CreateInstance(type, new object[] {this});
                SetSection(section);
            }
        }

        if (EditorApplication.isCompiling && !_isCompiling)
        {
            if (_section != null)
            {
                _section.Destroy();
                EditorPrefs.SetString("TilerLastWindow", _section.GetType().ToString());
            }
            _isCompiling = true;
        }
    }

    public void OnGUI()
    {
        if (_section != null)
            _section.OnGUI();
    }

    public void OnDestroy()
    {
        if (_section != null)
            _section.Destroy();

        _section = null;
    }

    public void OnEnable()
    {
        if (UpdateRequired())
        {
            if (EditorUtility.DisplayDialog("Update Required",
                                            "The latest verison of Tiler requires saved data to be updated before proceeding.\n" +
                                            "It is advised you backup before hitting update.\n", "Update", "Cancel"))
            {
                EditorPrefs.SetInt("TilerVersion", TilerVersion);

                UnityUpdate.PerformCheck();
            }
            else
            {
                _isClosing = true;
            }
        }

        wantsMouseMove = true;

        if (_section == null)
            return;

        _section.OnEnable();
    }

    public void OnDisable()
    {
        if (_section == null) return;
        _section.OnDisable(_isCompiling);
    }

    public void CloseLast()
    {
        if (_section != null)
            _section.Close();
    }

    public void SetSection(Section s)
    {
        _section = s;
    }

    private bool UpdateRequired()
    {
        var version = EditorPrefs.GetInt("TilerVersion", 0);

        if (version == TilerVersion)
            return false;

        var tilesets = UnityInternal.GetAssetsInProjectOfType<Tileset>().ToArray();

        // no tilesets to update anyway
        if (tilesets.Length == 0)
        {
            EditorPrefs.SetInt("TilerVersion", TilerVersion);
            return false;
        }

        return true;
    }
}