using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rack))]
public class RackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var rack = (Rack)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            rack.GenerateMesh();
    }
}
