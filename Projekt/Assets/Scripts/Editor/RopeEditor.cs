using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rope))]
public class RopeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var rope = (Rope)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            rope.Generate();
    }
}
