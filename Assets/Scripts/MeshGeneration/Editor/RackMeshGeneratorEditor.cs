using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RackMeshGenerator))]
public class RackMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var generator = (RackMeshGenerator)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            generator.GenerateMesh();
    }
}
