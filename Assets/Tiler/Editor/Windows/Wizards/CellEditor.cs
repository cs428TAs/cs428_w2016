using UnityEditor;

[CustomEditor(typeof(Cell))]
public class CellEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        /*var cell = (Cell) target;

        if (!cell.renderer.sharedMaterial) return;

        cell.renderer.sharedMaterial.renderQueue = EditorGUILayout.IntField("RenderQueue", cell.renderer.sharedMaterial.renderQueue);*/
    }
}