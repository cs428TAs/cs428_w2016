using System;
using UnityEngine;

[Serializable]
public class WindowPopup : PopupWindow
{
    private TilerWindow _window;

    public void Setup(TilerWindow window)
    {
        _window = window;
    }

    public override void OnGUI()
    {
        if (Event.current.type == EventType.mouseMove)
            Repaint();

        if (MenuOption("Draw"))
        {
            _window.CloseLast();
            _window.SetSection(new DrawWindow(_window));
            Close();
        }

        GUILayout.Space(5);

        if (MenuOption("Tileset"))
        {
            _window.CloseLast();
            _window.SetSection(new TilesetWindow(_window));
            Close();
            
        }

        base.OnGUI();
    }

    public bool MenuOption(string text)
    {
        var gc = new GUIContent(text);

        var rect = GUILayoutUtility.GetRect(gc, LabelStyle);

        if (rect.Contains(Event.current.mousePosition))
        {
            LabelStyle.normal.textColor = Color.red;
        }

        GUI.Label(rect, gc, LabelStyle);
        LabelStyle.normal.textColor = DefaultLabelColor;

        return MyGUI.ButtonMouseDown(rect);
    }
}