using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class HelpPopup : PopupWindow
{
    public void Setup(TilerWindow window)
    {
    }

    public override void OnGUI()
    {
        if (Event.current.type == EventType.mouseMove)
            Repaint();

        if (MenuOption("Unity Thread"))
        {
            EditorUtility.OpenWithDefaultApp("http://forum.unity3d.com/threads/186260-Tiler-tile-map-editor");
            Close();
        }

        GUILayout.Space(5);

        if (MenuOption("Tutorial"))
        {
            EditorUtility.OpenWithDefaultApp(Application.dataPath + "/Tiler/readme.pdf");
            Close();
        }

        GUILayout.Space(5);


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