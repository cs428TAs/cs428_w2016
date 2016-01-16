using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LayerPopup : PopupWindow
{
    public delegate void OnChange(int mask);
    public delegate void OnOrderChange();
    private OnChange _activeChange;
    private OnChange _visibleChange;
    private OnOrderChange _orderChange;
    private int _active;
    private int _mask;
    private List<TilerMap> _layers;

    public void Setup(int active, int mask, List<TilerMap> layers, OnChange activeChange, OnChange visibleChange, OnOrderChange orderChange)
    {
        _active = active;
        _mask = mask;
        _layers = layers;
        _activeChange = activeChange;
        _visibleChange = visibleChange;
        _orderChange = orderChange;
    }

    public void CallOnVisibleChange()
    {
        if (_visibleChange != null)
            _visibleChange(_mask);
    }
    public void CallOnActiveChangeChange()
    {
        if (_activeChange != null)
            _activeChange(_active);
    }

    public override void OnGUI()
    {
        for (int index = 0; index < _layers.Count; index++)
        {
            var layer = _layers[index];

            GUILayout.BeginHorizontal();
            if (MyGUI.HasToggled((_mask & 1 << index) == 1 << index, "", GUILayout.Width(16)))
            {
                if ((_mask & 1 << index) != 1 << index || _active != index)
                {
                    _mask ^= (1 << index);
                    CallOnVisibleChange();
                }
            }

            var gc = new GUIContent(layer.name);
            var rect = GUILayoutUtility.GetRect(gc, LabelStyle);
            if (index == _active)
            {
                LabelStyle.normal.textColor = Color.red;
            }
            GUI.Label(rect, gc, LabelStyle);
            LabelStyle.normal.textColor = DefaultLabelColor;

            if (MyGUI.ButtonMouseDown(rect))
            {
                if (_active != index)
                {
                    _mask |= (1 << index);
                    _active = index;
                    CallOnActiveChangeChange();
                    //Close();
                }
            }

            GUI.enabled = index != _layers.Count - 1;
            if (GUILayout.Button("D", EditorStyles.miniButtonLeft, GUILayout.Width(32)))
            {
                if (index == _active)
                    _active++;
                else if (index + 1 == _active)
                    _active--;

                GetValue(index, true);
            }
            
            GUI.enabled = index != 0;
            if (GUILayout.Button("U", EditorStyles.miniButtonRight, GUILayout.Width(32)))
            {
                if (index == _active)
                    _active--;
                else if (index - 1 == _active)
                    _active++;

                GetValue(index, false);
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }
        base.OnGUI();
    }

    private void GetValue(int index, bool down)
    {
        var shift = down ? 1 : -1;

        var tmp = _layers[index];
        _layers[index] = _layers[index + shift];
        _layers[index + shift] = tmp;

        // Reset render queue
        foreach (var c in _layers[index].Cells)
        {
            if (c.GetComponent<Renderer>() && c.GetComponent<Renderer>().sharedMaterial)
                c.GetComponent<Renderer>().sharedMaterial.renderQueue += _layers[index].Layer;
        }

        foreach (var c in _layers[index + shift].Cells)
        {
            if (c.GetComponent<Renderer>() && c.GetComponent<Renderer>().sharedMaterial)
                c.GetComponent<Renderer>().sharedMaterial.renderQueue += _layers[index + shift].Layer;
        }

        _layers[index].Layer = _layers[index + shift].Layer;
        _layers[index + shift].Layer = _layers[index + shift].Layer + shift;

        // Set render queue
        foreach (var c in _layers[index].Cells)
        {
            if (c.GetComponent<Renderer>() && c.GetComponent<Renderer>().sharedMaterial)
                c.GetComponent<Renderer>().sharedMaterial.renderQueue -= _layers[index].Layer;
        }

        foreach (var c in _layers[index + shift].Cells)
        {
            if (c.GetComponent<Renderer>() && c.GetComponent<Renderer>().sharedMaterial)
                c.GetComponent<Renderer>().sharedMaterial.renderQueue -= _layers[index + shift].Layer;
        }

        _orderChange();
        GUIUtility.ExitGUI();
    }
}