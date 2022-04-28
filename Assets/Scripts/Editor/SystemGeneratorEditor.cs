using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SystemGenerator), true)]
public class SystemGeneratorEditor : Editor
{
    private string formula = "x + 2";

    public override void OnInspectorGUI()
    {
        var generator = (SystemGenerator)target;

        base.DrawDefaultInspector();

        formula = GUILayout.TextArea(formula);

        if (GUILayout.Button("Generate"))
        {
            var node = MathParser.Parser.Parse(formula);
            generator.Generate(node, -5, 5, -5, 5);
        }
    }
}
