using UnityEngine;

public static class MyGUI
{
    public static bool HasToggled(bool state, string text, params GUILayoutOption[] options)
    {
        var newState = GUILayout.Toggle(state, text, options);

        if (state != newState)
            return true;

        return false;
    }

    public static bool HasToggledPositive(bool state, string text, GUIStyle style, params GUILayoutOption[] options)
    {
        var newState = GUILayout.Toggle(state, text, style, options);

        if (state != newState)
            return newState;

        return false;
    }

    public static bool ButtonMouseDown(Rect position, int mouseButton = 0)
    {
        Event current = Event.current;
        EventType type = current.type;

        if (type == EventType.MouseDown && position.Contains(current.mousePosition) && current.button == mouseButton)
        {
            Event.current.Use();
            return true;
        }


        return false;
    }
}
