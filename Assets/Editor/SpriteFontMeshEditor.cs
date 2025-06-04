using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteFontMesh))]
public class SpriteFontMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpriteFontMesh script = (SpriteFontMesh)target;

        if (GUILayout.Button("Generate Text"))
        {
            script.ValidateColors();
            script.GenerateText(script.textToDisplay, script.percent);
            EditorUtility.SetDirty(script);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
