using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NumberLine))]
public class NumberLineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var numberLine = (NumberLine)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            numberLine.Generate();
    }
}
