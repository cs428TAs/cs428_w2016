using System;
using UnityEngine;

[Serializable]
public class FileMenuPopup : PopupWindow
{
    private Section _parent;
    
    public void Setup(Section parent)
    {
        _parent = parent;
    }

    public override void OnGUI()
    {
        if (Event.current.type == EventType.mouseMove)
            Repaint();

        if (Menu("New"))
        {
            _parent.New();
            Close();
        }
        if (Menu("Save"))
        {
            _parent.Save();
            Close();
        }

        base.OnGUI();
    }

    private bool Menu(string optionName)
    {
        var gc = new GUIContent(optionName);

        var rect = GUILayoutUtility.GetRect(gc, LabelStyle);

        if (rect.Contains(Event.current.mousePosition))
        {
            LabelStyle.normal.textColor = Color.red;
        }

        GUI.Label(rect, gc, LabelStyle);
        LabelStyle.normal.textColor = DefaultLabelColor;

        if (MyGUI.ButtonMouseDown(rect))
        {
            return true;
        }
        return false;
    }
}