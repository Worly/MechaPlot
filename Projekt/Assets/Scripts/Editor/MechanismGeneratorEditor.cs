using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MechanismGenerator), true)]
public class MechanismGeneratorEditor : Editor
{
    private string formula = "x + 2";

    public override void OnInspectorGUI()
    {
        var generator = (MechanismGenerator)target;

        base.DrawDefaultInspector();

        formula = GUILayout.TextArea(formula);

        if (GUILayout.Button("Generate"))
        {
            var node = MathParser.Parser.Parse(formula);
            generator.Generate(node, -5, 5, -5, 5);
        }
    }
}
