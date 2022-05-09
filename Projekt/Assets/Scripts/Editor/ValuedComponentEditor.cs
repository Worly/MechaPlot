using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ValuedComponent), true)]
public class ValuedComponentEditor: Editor
{
    public override void OnInspectorGUI()
    {
        var generator = (ValuedComponent)target;

        base.DrawDefaultInspector();
    }
}
