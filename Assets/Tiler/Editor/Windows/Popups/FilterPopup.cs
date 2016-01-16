using Tiler;
using Tiler.Editor;
using UnityEngine;

public class FilterPopup : PopupWindow
{
    public delegate void OnChange();
    private OnChange _onChange;

    private Filter _filter;

    public void Setup(Filter filter, OnChange change)
    {
        _filter = filter;
        _onChange = change;
    }

    public void CallOnChange()
    {
        if (_onChange != null)
            _onChange();
    }

    public override void OnGUI()
    {
        if (MyGUI.HasToggled(_filter.IsFilterWithRotation, "Rotate"))
        {
            _filter.IsFilterWithRotation = !_filter.IsFilterWithRotation;
            CallOnChange();
        }

        if (MyGUI.HasToggled(_filter.IsFilterExclusive, "Exclusive"))
        {
            _filter.IsFilterExclusive = !_filter.IsFilterExclusive;
            CallOnChange();
        }

        GUILayout.Space(5);

        var c = _filter.ConnectionFilter;

        if (MyGUI.HasToggled((c & ConnectionMask.Left) == ConnectionMask.Left, "Left"))
            c ^= (ConnectionMask)(1 << 0);

        if (MyGUI.HasToggled((c & ConnectionMask.Top) == ConnectionMask.Top, "Top"))
            c ^= (ConnectionMask)(1 << 1);

        if (MyGUI.HasToggled((c & ConnectionMask.Right) == ConnectionMask.Right, "Right"))
            c ^= (ConnectionMask)(1 << 2);

        if (MyGUI.HasToggled((c & ConnectionMask.Bottom) == ConnectionMask.Bottom, "Bottom"))
            c ^= (ConnectionMask)(1 << 3);

        if (c != _filter.ConnectionFilter)
        {
            _filter.ConnectionFilter = c;
            CallOnChange();
        }

        base.OnGUI();
    }
}