using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberLine : MonoBehaviour
{
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private TextMeshPro textPrefab;
    [SerializeField] private Direction direction;
    [SerializeField] public float fromValue;
    [SerializeField] public float toValue;
    [SerializeField] private float length;
    [SerializeField] private float divisionLen;
    [SerializeField] private float mainTickLen;
    [SerializeField] private float lineEndsOffset;
    [SerializeField] private float arrowSize;

    public void Clear()
    {
        var list = new List<Transform>();
        foreach (Transform t in transform)
            list.Add(t);

        list.ForEach(t => t.parent = null);
        list.ForEach(t => DestroyImmediate(t.gameObject));
    }

    public void Generate()
    {
        Clear();

        Vector3 mainDirection;
        Vector3 secondDirection;

        if (direction == Direction.Horizontal)
        {
            mainDirection = Vector3.right;
            secondDirection = Vector3.up;
        }
        else
        {
            mainDirection = Vector3.up;
            secondDirection = Vector3.right;
        }

        int numberOfDivisions = Mathf.RoundToInt(length / divisionLen);
        float valueDelta = toValue - fromValue;
        var divisionDelta = valueDelta / numberOfDivisions;

        // Add main line
        var mainLine = Instantiate(linePrefab, transform);
        mainLine.positionCount = 2;
        mainLine.SetPosition(0, -mainDirection * (length / 2 + lineEndsOffset));
        mainLine.SetPosition(1, mainDirection * (length / 2 + lineEndsOffset));

        // Add ticks
        for (int i = 0; i <= numberOfDivisions; i++)
        {
            var percentage = (float)i / numberOfDivisions;
            var tickPos = mainDirection * (-length / 2 + length * percentage);

            var tick = Instantiate(linePrefab, transform);
            tick.positionCount = 2;
            tick.SetPosition(0, tickPos);
            tick.SetPosition(1, tickPos + secondDirection * mainTickLen);

            var valueText = Instantiate(textPrefab, transform);
            valueText.text = (fromValue + divisionDelta * i).ToString("0.##");
            valueText.transform.localPosition = tickPos + secondDirection * (mainTickLen + 0.8f);

            if (direction == Direction.Vertical)
            {
                valueText.alignment = TextAlignmentOptions.Left;
                valueText.rectTransform.localPosition += new Vector3(valueText.rectTransform.sizeDelta.x / 2f - 0.5f, 0, 0);
            }
        }

        // Add arrow
        var endOfLine = mainDirection * (length / 2 + lineEndsOffset);
        var arrowLine1 = Instantiate(linePrefab, transform);
        arrowLine1.positionCount = 2;
        arrowLine1.SetPosition(0, endOfLine);
        arrowLine1.SetPosition(1, endOfLine - mainDirection * arrowSize + secondDirection * arrowSize * 0.5f);
        var arrowLine2 = Instantiate(linePrefab, transform);
        arrowLine2.positionCount = 2;
        arrowLine2.SetPosition(0, endOfLine);
        arrowLine2.SetPosition(1, endOfLine - mainDirection * arrowSize - secondDirection * arrowSize * 0.5f);
    }

    public enum Direction
    {
        Horizontal,
        Vertical
    }
}


