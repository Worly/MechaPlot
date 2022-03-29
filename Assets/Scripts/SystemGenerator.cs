using MathParser;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Gear gearPrefab;
    [SerializeField] private Crank crankPrefab;
    [SerializeField] private Differential differentialPrefab;
    [SerializeField] private Multiplier multiplierPrefab;
    [SerializeField] private Plotter plotterPrefab;
    [SerializeField] private Rope ropePrefab;

    private Crank inputCrank;
    private float minZValue;

    private readonly float MAX_X_VALUE = -10;

    public void Clear()
    {
        foreach (Transform t in transform)
            DestroyImmediate(t.gameObject);

        minZValue = float.MaxValue;
    }

    public void Generate(Node topNode)
    {
        Clear();
        CreateInputCrank();

        var outputGear = GenerateRecursive(topNode, 0);

        AddPlotter(outputGear);

        inputCrank.transform.position = new Vector3(inputCrank.transform.position.x, inputCrank.transform.position.y, minZValue - 5f);
    }

    private void CreateInputCrank()
    {
        inputCrank = Instantiate(crankPrefab, transform);
        inputCrank.transform.localPosition = Vector3.zero;
    }

    private Gear GenerateRecursive(Node node, float requiredZPosition)
    {
        if (node is InputNode)
        {
            var gear = Instantiate(gearPrefab, transform);

            var inputCrankPosition = inputCrank.transform.localPosition;
            gear.SetPosition(new Vector3(inputCrankPosition.x, inputCrankPosition.y, requiredZPosition));

            if (requiredZPosition < minZValue)
                minZValue = requiredZPosition;

            gear.InputComponent = inputCrank.Gear;
            gear.onlyCopyInput = true;

            return gear;
        }

        if (node is ValueNode valueNode)
        {
            var gear = Instantiate(gearPrefab, transform);
            gear.transform.localPosition = new Vector3(MAX_X_VALUE, 0, requiredZPosition);
            gear.Value = valueNode.Value;
            return gear;
        }

        if (node is OperationNode operationNode)
        {
            if (operationNode.Operation == Operation.ADDITION)
            {
                var differential = Instantiate(differentialPrefab, transform);
                differential.transform.localPosition = new Vector3(-30, 0, requiredZPosition - differential.OutputGear.transform.localPosition.z);

                var inputGear1Position = this.transform.InverseTransformPoint(differential.InputGear1.transform.position);
                var inputGear2Position = this.transform.InverseTransformPoint(differential.InputGear2.transform.position);

                var leftGear = GenerateRecursive(operationNode.Left, inputGear1Position.z);
                var rightGear = GenerateRecursive(operationNode.Right, inputGear2Position.z);

                differential.InputGear1.InputComponent = leftGear;
                differential.InputGear1.Circumference = leftGear.Circumference;

                differential.InputGear2.InputComponent = rightGear;
                differential.InputGear2.Circumference = rightGear.Circumference;

                var leftGearEdge = leftGear.GetPositionOfEdge(-transform.right);
                var rightGearEdge = rightGear.GetPositionOfEdge(-transform.right);
                if (leftGearEdge.x < rightGearEdge.x)
                {
                    var gearRightEdge = differential.InputGear1.GetPositionOfEdge(transform.right);
                    var positionOfGear = leftGearEdge + differential.InputGear1.transform.position - gearRightEdge;

                    MoveParentWithChildCoordinates(differential.transform, differential.InputGear1.transform, positionOfGear);

                    MakeBeltConnection(rightGear, differential.InputGear2);
                }
                else if (leftGearEdge.x > rightGearEdge.x)
                {
                    var gearRightEdge = differential.InputGear2.GetPositionOfEdge(transform.right);
                    var positionOfGear = rightGearEdge + differential.InputGear2.transform.position - gearRightEdge;

                    MoveParentWithChildCoordinates(differential.transform, differential.InputGear2.transform, positionOfGear);

                    MakeBeltConnection(leftGear, differential.InputGear1);
                }
                else
                {
                    var gearRightEdge = differential.InputGear1.GetPositionOfEdge(transform.right);
                    var positionOfGear = leftGearEdge + differential.InputGear1.transform.position - gearRightEdge;

                    MoveParentWithChildCoordinates(differential.transform, differential.InputGear1.transform, positionOfGear);
                }

                return differential.OutputGear;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        throw new NotImplementedException();
    }

    private void AddPlotter(Gear outputGear)
    {
        var plotter = Instantiate(plotterPrefab, transform);

        plotter.InputGearY.InputComponent = outputGear;
        plotter.InputGearX.InputComponent = inputCrank.Gear;

        var outputGearEdge = outputGear.GetPositionOfEdge(-transform.right);
        var inputGearYEdge = plotter.InputGearY.GetPositionOfEdge(transform.right);
        var inputGearYPosition = outputGearEdge + plotter.InputGearY.transform.position - inputGearYEdge;

        MoveParentWithChildCoordinates(plotter.transform, plotter.InputGearY.transform, inputGearYPosition);
    }

    private void MakeBeltConnection(Gear fromGear, Gear toGear)
    {
        var gearTake = Instantiate(gearPrefab, transform);
        gearTake.Circumference = 10;
        gearTake.InputComponent = fromGear;
        gearTake.PlaceOn(fromGear, transform.up);

        var takeBelt = Instantiate(gearPrefab, transform);
        takeBelt.GearMeshGenerator.gearType = GearType.Belt;
        takeBelt.Circumference = 10;
        takeBelt.InputComponent = gearTake;
        takeBelt.onlyCopyInput = true;
        takeBelt.PlaceNextTo(gearTake, -transform.forward);

        var giveBelt = Instantiate(gearPrefab, transform);
        giveBelt.GearMeshGenerator.gearType = GearType.Belt;
        giveBelt.Circumference = toGear.Circumference;
        giveBelt.InputComponent = takeBelt;
        giveBelt.PlaceNextTo(toGear, -transform.forward);

        toGear.InputComponent = giveBelt;
        toGear.onlyCopyInput = true;

        var belt = Instantiate(ropePrefab, transform);
        belt.SetGears(takeBelt, giveBelt);
    }

    /// <summary>
    /// Sets parents position so that child's global position is exactly <paramref name="positionOfChild"/>.
    /// </summary>
    private void MoveParentWithChildCoordinates(Transform parent, Transform child, Vector3 positionOfChild)
    {
        parent.position = positionOfChild - parent.InverseTransformPoint(child.position);
    }
}
