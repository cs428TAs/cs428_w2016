using System;
using System.Globalization;
using Tiler;
using UnityEditor;
using UnityEngine;

[Serializable]
public class BrushWindow : IChildWindow
{
    private DrawWindow _draw;

    public BrushWindow(DrawWindow draw)
    {
        _draw = draw;
    }

    public void OnGUI()
    {
        if (_draw == null) return;

        Toolbar();

        var preview = _draw.DrawTool.GetBrush().GetPreview();

        if (preview == null) return;

        //var min = Mathf.Min(position.width, position.height);
        var min = 212;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        var rect = GUILayoutUtility.GetRect(min, min);
        GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void Toolbar()
    {
        GUILayout.BeginHorizontal("toolbar");

        var tool = _draw.DrawTool;
        var brush = _draw.DrawTool.GetBrush();

        if (tool is PaintTool)
        {
            if (brush is NormalBrush)
            {
                NormalBrushOptions();
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Rotate", EditorStyles.toolbarButton))
        {
            brush.Rotate();
        }

        GUILayout.EndHorizontal();
    }

    public void BrushSizeAdd(int i)
    {
        var brush = _draw.DrawTool.GetBrush();

        var p = new Point
        {
            X = Mathf.Clamp(brush.BrushSize.X + i, 1, 9),
            Y = Mathf.Clamp(brush.BrushSize.Y + i, 1, 9)
        };

        brush.BrushSize = p;

        _draw.Repaint();
    }

    private void NormalBrushOptions()
    {
        var brush = _draw.DrawTool.GetBrush();

        if (GUILayout.Button("<", EditorStyles.toolbarButton))
        {
            BrushSizeAdd(-1);
        }

        GUILayout.Label(brush.BrushSize.X.ToString(CultureInfo.InvariantCulture));

        if (GUILayout.Button(">", EditorStyles.toolbarButton))
        {
            BrushSizeAdd(1);
        }
    }
}