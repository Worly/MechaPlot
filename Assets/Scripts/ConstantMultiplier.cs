using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMultiplier : MonoBehaviour
{
    [SerializeField] private Gear gearPrefab;
    [SerializeField] private Rope ropePrefab;

    [SerializeField] private Gear inputGear;
    [SerializeField] private Gear outputGear;

    [SerializeField] private float constant = 1f;

    public float Constant => constant;
    public Gear InputGear => inputGear;
    public Gear OutputGear => outputGear;

    private readonly int smallerGearCircumference = 10;
    private readonly int maxStepRatio = 5;

    public void Clear()
    {
        var list = new List<Transform>();
        foreach (Transform t in transform)
        {
            if (t != inputGear.transform && t != outputGear.transform)
                list.Add(t);
        }

        list.ForEach(t => t.parent = null);
        list.ForEach(t => DestroyImmediate(t.gameObject));
    }

    public void GenerateGears(float constant)
    {
        Clear();

        this.constant = constant;

        var shouldNegative = false;
        var isNegativeCurrent = true;
        var isReciprocal = false;

        Gear lastGear = inputGear;
        decimal leftValue = (decimal)constant;

        if (leftValue < 0)
        {
            leftValue *= -1;
            shouldNegative = true;
        }

        if (leftValue < 1)
        {
            leftValue = 1 / leftValue;
            isReciprocal = true;
        }

        while (leftValue > 1)
        {
            decimal ratio;

            if (leftValue >= 5)
                ratio = 5;
            else
                ratio = leftValue;

            decimal firstGearCircumference = ratio * smallerGearCircumference;
            decimal secondGearCircumference = smallerGearCircumference;

            bool isWholeRatio = firstGearCircumference == Math.Floor(firstGearCircumference);

            if (isReciprocal)
                Swap(ref firstGearCircumference, ref secondGearCircumference);

            var firstGear = Instantiate(gearPrefab, transform);
            firstGear.Circumference = (float)firstGearCircumference;
            firstGear.InputComponent = lastGear;
            firstGear.onlyCopyInput = true;
            firstGear.PlaceNextTo(lastGear, transform.forward);

            var secondGear = Instantiate(gearPrefab, transform);
            secondGear.Circumference = (float)secondGearCircumference;
            secondGear.InputComponent = firstGear;
            secondGear.PlaceOn(firstGear, -transform.right);

            if (!isWholeRatio)
            {
                firstGear.GearMeshGenerator.gearType = GearType.Belt;
                secondGear.GearMeshGenerator.gearType = GearType.Belt;

                firstGear.GenerateMesh();
                secondGear.GenerateMesh();

                var rope = Instantiate(ropePrefab, transform);
                rope.SetGears(firstGear, secondGear);
            }

            if (isWholeRatio)
                isNegativeCurrent = !isNegativeCurrent;

            lastGear = secondGear;
            leftValue /= ratio;
        }

        if (isNegativeCurrent != shouldNegative)
        {
            if (lastGear.GearMeshGenerator.gearType == GearType.Belt)
            {
                var transferGear = Instantiate(gearPrefab, transform);
                transferGear.Circumference = 25;
                transferGear.InputComponent = lastGear;
                transferGear.onlyCopyInput = true;
                transferGear.PlaceNextTo(lastGear, transform.forward);

                lastGear = transferGear;
            }

            var flipGear = Instantiate(gearPrefab, transform);
            flipGear.Circumference = lastGear.Circumference;
            flipGear.InputComponent = lastGear;
            flipGear.PlaceOn(lastGear, -transform.right);

            lastGear = flipGear;
        }

        outputGear.InputComponent = lastGear;
        outputGear.onlyCopyInput = true;
        outputGear.PlaceNextTo(lastGear, transform.forward);
    }

    private void Swap<T>(ref T first, ref T second)
    {
        var temp = first;
        first = second;
        second = temp;
    }
}
