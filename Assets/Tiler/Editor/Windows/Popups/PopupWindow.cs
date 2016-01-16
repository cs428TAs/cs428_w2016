using Tiler;
using UnityEditor;
using UnityEngine;

public abstract class PopupWindow : EditorWindow
{
    protected static GUIStyle LabelStyle;

    private static Color _defaultColor;
    protected static Color DefaultLabelColor { get
    {
        if (_defaultColor == new Color())
            _defaultColor = GUI.skin.label.normal.textColor;
        
        return _defaultColor;
    } }
    private bool _doClose;

    private void Init(Rect pos)
    {
        var vec2 = new Vector2(pos.x, pos.y);

        Vector2 vector2 = GUIUtility.GUIToScreenPoint(vec2);
        UnityInternal.ShowWithModePopup(this);
        position = new Rect
        {
            x = vector2.x,
            y = vector2.y,
            width = pos.width,
            height = pos.height
        };

        minSize = new Vector2(position.width, position.height);
        maxSize = new Vector2(position.width, position.height);

        wantsMouseMove = true;

        if (Application.platform == RuntimePlatform.OSXEditor)
            Focus();
    }

    public virtual void Update()
    {
        if (_doClose)
        {
            _doClose = false;
            Close();
        }
    }

    public virtual void OnGUI()
    {
        GUI.Label(new Rect(0.0f, 0.0f, position.width, position.height), GUIContent.none, "grey_border");
    }

    public virtual void OnEnable()
    {
        LabelStyle = new GUIStyle(EditorStyles.label);
    }

    public virtual void OnLostFocus()
    {
        Cancel();
    }

    private void Cancel()
    {
        _doClose = true;
        GUI.changed = true;
    }

    public static T ShowAtPosition<T>(Rect pos)
        where T: PopupWindow
    {
        var window = CreateInstance<T>();
        window.Init(pos);

        return window;
    }
}