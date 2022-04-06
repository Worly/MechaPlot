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
        var list = new List<Transform>();
        foreach (Transform t in transform)
            list.Add(t);

        list.ForEach(t => t.parent = null);
        list.ForEach(t => DestroyImmediate(t.gameObject));

        minZValue = float.MaxValue;
    }

    public void Generate(Node topNode)
    {
        Clear();

        MeshGenerationManager.Pause();

        CreateInputCrank();

        var outputGear = GenerateRecursive(topNode, 0);
        inputCrank.transform.position = new Vector3(inputCrank.transform.position.x, inputCrank.transform.position.y, minZValue - 10f);

        AddPlotter(outputGear);

        MeshGenerationManager.UnPause();

        Debug.Log("Generated!");
    }

    private void CreateInputCrank()
    {
        inputCrank = Instantiate(crankPrefab, transform);
        inputCrank.transform.localPosition = Vector3.zero;
    }

    private Gear GenerateRecursive(Node node, float requiredZPosition)
    {
        Debug.Log("Doing " + node.ToString());
        if (node is InputNode)
        {
            var gear = Instantiate(gearPrefab, transform);

            var inputCrankPosition = inputCrank.transform.localPosition;
            gear.SetPositionLocal(new Vector3(inputCrankPosition.x, inputCrankPosition.y, requiredZPosition));

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
            else if (operationNode.Operation == Operation.MULTIPLICATION)
            {
                var multiplier = Instantiate(multiplierPrefab, transform);
                multiplier.transform.localPosition = new Vector3(-30, 0, requiredZPosition - multiplier.OutputGear.transform.localPosition.z);

                var inputGear1Position = this.transform.InverseTransformPoint(multiplier.InputGear1.transform.position);
                var inputGear2Position = this.transform.InverseTransformPoint(multiplier.InputGear2.transform.position);

                var leftGear = GenerateRecursive(operationNode.Left, inputGear1Position.z);
                var rightGear = GenerateRecursive(operationNode.Right, inputGear2Position.z);

                multiplier.InputGear1.InputComponent = leftGear;
                multiplier.InputGear1.Circumference = leftGear.Circumference;

                multiplier.InputGear2.InputComponent = rightGear;
                multiplier.InputGear2.Circumference = rightGear.Circumference;

                var leftGearEdge = leftGear.GetPositionOfEdge(-transform.right);
                var rightGearEdge = rightGear.GetPositionOfEdge(-transform.right);
                if (leftGearEdge.x < rightGearEdge.x)
                {
                    var gearRightEdge = multiplier.InputGear1.GetPositionOfEdge(transform.right);
                    var positionOfGear = leftGearEdge + multiplier.InputGear1.transform.position - gearRightEdge;

                    MoveParentWithChildCoordinates(multiplier.transform, multiplier.InputGear1.transform, positionOfGear);

                    MakeBeltConnection(rightGear, multiplier.InputGear2, backSide: true);
                }
                else if (leftGearEdge.x > rightGearEdge.x)
                {
                    var gearRightEdge = multiplier.InputGear2.GetPositionOfEdge(transform.right);
                    var positionOfGear = rightGearEdge + multiplier.InputGear2.transform.position - gearRightEdge;

                    MoveParentWithChildCoordinates(multiplier.transform, multiplier.InputGear2.transform, positionOfGear);

                    MakeBeltConnection(leftGear, multiplier.InputGear1);
                }
                else
                {
                    var gearRightEdge = multiplier.InputGear1.GetPositionOfEdge(transform.right);
                    var positionOfGear = leftGearEdge + multiplier.InputGear1.transform.position - gearRightEdge;

                    MoveParentWithChildCoordinates(multiplier.transform, multiplier.InputGear1.transform, positionOfGear);
                }

                return multiplier.OutputGear;
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
        plotter.InputGearY.Circumference = outputGear.Circumference;

        var outputGearEdge = outputGear.GetPositionOfEdge(-transform.right);
        var inputGearYEdge = plotter.InputGearY.GetPositionOfEdge(transform.right);
        var inputGearYPosition = outputGearEdge + plotter.InputGearY.transform.position - inputGearYEdge;

        MoveParentWithChildCoordinates(plotter.transform, plotter.InputGearY.transform, inputGearYPosition);

        var xTransferGear = Instantiate(gearPrefab, transform);
        var inputGearXPosition = plotter.InputGearX.transform.position;
        xTransferGear.SetPositionGlobal(new Vector3(inputGearXPosition.x, inputGearXPosition.y, inputCrank.transform.position.z));
        plotter.InputGearX.InputComponent = xTransferGear;
        plotter.InputGearX.onlyCopyInput = true;

        MakeBeltConnection(inputCrank.Gear, xTransferGear, backSide: true, topSide: false);
    }

    private void MakeBeltConnection(Gear fromGear, Gear toGear, bool backSide = false, bool topSide = true)
    {
        int zSide = backSide ? 1 : -1;
        int ySide = topSide ? 1 : -1;

        var gearTake = Instantiate(gearPrefab, transform);
        gearTake.Circumference = 10;
        gearTake.InputComponent = fromGear;
        gearTake.PlaceOn(fromGear, ySide * transform.up);

        var takeBelt = Instantiate(gearPrefab, transform);
        takeBelt.GearMeshGenerator.gearType = GearType.Belt;
        takeBelt.Circumference = 10;
        takeBelt.InputComponent = gearTake;
        takeBelt.onlyCopyInput = true;
        takeBelt.PlaceNextTo(gearTake, zSide * transform.forward);

        var giveBelt = Instantiate(gearPrefab, transform);
        giveBelt.GearMeshGenerator.gearType = GearType.Belt;
        giveBelt.Circumference = toGear.Circumference;
        giveBelt.InputComponent = takeBelt;
        giveBelt.PlaceNextTo(toGear, zSide * transform.forward);

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
