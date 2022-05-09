using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConstantMultiplier), true)]
public class ConstantMultiplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var constantMultiplier = (ConstantMultiplier)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("GenerateGears"))
            constantMultiplier.GenerateGears(constantMultiplier.Constant);
    }
}
