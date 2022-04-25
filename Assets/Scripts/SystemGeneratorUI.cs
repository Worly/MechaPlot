using MathParser;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemGeneratorUI : MonoBehaviour
{
    [SerializeField] private InputField functionInputField;
    [SerializeField] private InputField xFromInputField;
    [SerializeField] private InputField xToInputField;
    [SerializeField] private InputField yFromInputField;
    [SerializeField] private InputField yToInputField;

    [SerializeField] private Text errorText;

    [SerializeField] private SystemGenerator systemGenerator;

    public void Start()
    {
        errorText.text = "";
    }

    public void Generate()
    {
        float xFrom, xTo, yFrom, yTo;

        errorText.text = "";

        if (ValidateEmpty(functionInputField.text, "Mathematical function"))
            return;
        if (ValidateFloat(xFromInputField.text, "Plotter range X 'from'", out xFrom))
            return;
        if (ValidateFloat(xToInputField.text, "Plotter range X 'to'", out xTo))
            return;
        if (ValidateFloat(yFromInputField.text, "Plotter range Y 'from'", out yFrom))
            return;
        if (ValidateFloat(yToInputField.text, "Plotter range Y 'to'", out yTo))
            return;

        if (xFrom >= xTo)
        {
            errorText.text = "Plotter range X 'from' must be less than 'to'!";
            return;
        }

        if (yFrom >= yTo)
        {
            errorText.text = "Plotter range Y 'from' must be less than 'to'!";
            return;
        }

        Node expressionTopNode;

        try
        {
            expressionTopNode = Parser.Parse(functionInputField.text);
        }
        catch (Exception e)
        {
            errorText.text = "Error while parsing the function: " + e.Message;
            return;
        }

        try
        {
            systemGenerator.Generate(expressionTopNode);
        }
        catch (Exception e)
        {
            errorText.text = "Error while generating mechanism: " + e.Message;
            return;
        }
    }

    private bool ValidateEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            errorText.text = $"{name} cannot be empty!";
            return true;
        }

        return false;
    }

    private bool ValidateFloat(string s, string name, out float value)
    {
        value = 0;
        if (ValidateEmpty(s, name))
            return true;

        if(!float.TryParse(s, out value))
        {
            errorText.text = $"{name} must be a decimal number!";
            return true;
        }

        return false;
    }
}
