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
    [SerializeField] private Color errorColor;
    [SerializeField] private Color warningColor;

    [SerializeField] private MechanismGenerator systemGenerator;

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
            SetError("Plotter range X 'from' must be less than 'to'!");
            return;
        }

        if (yFrom >= yTo)
        {
            SetError("Plotter range Y 'from' must be less than 'to'!");
            return;
        }

        Node expressionTopNode;

        try
        {
            expressionTopNode = Parser.Parse(functionInputField.text);
        }
        catch (Exception e)
        {
            SetError("Error while parsing the function: " + e.Message);
            Debug.LogError(e);
            return;
        }

        try
        {
            expressionTopNode = expressionTopNode.LimitMultiplication(xFrom, xTo);
        }
        catch (Exception e)
        {
            SetError("Error while limiting multiplication: " + e.Message);
            Debug.LogError(e);
            return;
        }


        try
        {
            expressionTopNode.FindExtremes(xFrom, xTo);

            bool changedRange = false;

            if (expressionTopNode.MinValue < yFrom)
            {
                yFrom = expressionTopNode.MinValue;
                yFromInputField.text = yFrom.ToString();
                changedRange = true;
            }

            if (expressionTopNode.MaxValue > yTo)
            {
                yTo = expressionTopNode.MaxValue;
                yToInputField.text = yTo.ToString();
                changedRange = true;
            }

            if (changedRange)
                SetWarning("Warning: Y range changed to maximum and minimum of a function!");

            systemGenerator.Generate(expressionTopNode, xFrom, xTo, yFrom, yTo);
        }
        catch (Exception e)
        {
            SetError("Error while generating mechanism: " + e.Message);
            Debug.LogError(e);
            return;
        }
    }

    private bool ValidateEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            SetError($"{name} cannot be empty!");
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
            SetError($"{name} must be a decimal number!");
            return true;
        }

        return false;
    }

    private void SetError(string error)
    {
        errorText.text = error;
        errorText.color = errorColor;
    }

    private void SetWarning(string warning)
    {
        errorText.text = warning;
        errorText.color = warningColor;
    }
}
