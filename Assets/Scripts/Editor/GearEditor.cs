using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Gear), true)]
public class GearEditor: Editor
{
    public override void OnInspectorGUI()
    {
        var gear = (Gear)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("GenerateMesh"))
            gear.GenerateMesh();
    }
}
