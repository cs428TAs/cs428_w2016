using UnityEditor;
using UnityEngine;

public abstract class Section
{
    protected const int WindowPadding = 2;
    protected const int ToolbarHeight = 17;
    protected const int WindowTitle = 20;
    protected const int ScrollBarWidth = 16;

    public TilerWindow Parent;
    public bool IsClosed { get; set; }
    private static GUIStyle _windowNoTitleSTyle;
    protected static GUIStyle WindowNoTitleStyle
    {
        get
        {
            if (_windowNoTitleSTyle == null)
                SetupStyles();
            return _windowNoTitleSTyle;
        }
    }
    private static GUIStyle _windowTitleSTyle;
    protected static GUIStyle WindowTitleStyle
    {
        get
        {
            if (_windowTitleSTyle == null)
                SetupStyles();
            return _windowTitleSTyle;
        }
    }

    private static void SetupStyles()
    {
        _windowNoTitleSTyle = new GUIStyle(GUI.skin.window);
        _windowNoTitleSTyle.padding = new RectOffset(2,2,2,2);
        _windowTitleSTyle = new GUIStyle(GUI.skin.window);
        _windowTitleSTyle.padding = new RectOffset(2, 2, 20, 2);
    }

    protected Section(TilerWindow parent)
    {
        Parent = parent;
    }

    public virtual void OnGUI()
    {
        GUILayout.BeginHorizontal("toolbar");
        ToolBar();
        GUILayout.EndHorizontal();
    }

    public virtual void Repaint()
    {
        Parent.Repaint();
    }

    public virtual void OnEnable()
    {
        
    }

    public virtual void OnDisable(bool compiling)
    {
        if (!compiling && !EditorApplication.isCompiling)
        {
            Save();
        }
    }

    public virtual void Destroy()
    {
        IsClosed = true;
    }
    public virtual void Close()
    {
        OnDisable(false);
    }

    public virtual void ToolBar()
    {
        GUILayout.Box("File", EditorStyles.toolbarDropDown);
        Rect fileRect = GUILayoutUtility.GetLastRect();

        if (MyGUI.ButtonMouseDown(fileRect))
        {
            var win = PopupWindow.ShowAtPosition<FileMenuPopup>(new Rect(fileRect.x, fileRect.y + fileRect.height, 50f, 60f));
            win.Setup(this);
            GUIUtility.ExitGUI();
        }

        GUILayout.Box("Window", EditorStyles.toolbarDropDown);
        Rect windowRect = GUILayoutUtility.GetLastRect();

        if (MyGUI.ButtonMouseDown(windowRect))
        {
            var win = PopupWindow.ShowAtPosition<WindowPopup>(new Rect(windowRect.x, windowRect.y + windowRect.height, 50f, 60f));
            win.Setup(Parent);
            GUIUtility.ExitGUI();
        }

        GUILayout.Box("Help", EditorStyles.toolbarDropDown);
        Rect helpRect = GUILayoutUtility.GetLastRect();

        if (MyGUI.ButtonMouseDown(helpRect))
        {
            var win = PopupWindow.ShowAtPosition<HelpPopup>(new Rect(helpRect.x, helpRect.y + windowRect.height, 100f, 60f));
            win.Setup(Parent);
            GUIUtility.ExitGUI();
        }

        GUILayout.FlexibleSpace();
    }

    public abstract void New();
    public abstract void Save();
}
