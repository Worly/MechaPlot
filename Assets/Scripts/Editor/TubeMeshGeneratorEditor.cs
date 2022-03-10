using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TubeMeshGenerator))]
public class TubeMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var generator = (TubeMeshGenerator)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            generator.GenerateMesh();
    }
}
