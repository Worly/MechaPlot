using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GearMeshGenerator))]
public class GearMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var generator = (GearMeshGenerator)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            generator.Generate();
    }
}
