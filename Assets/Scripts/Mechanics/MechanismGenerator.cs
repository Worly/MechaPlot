using MathParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MechanismGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Gear gearPrefab;
    [SerializeField] private Crank crankPrefab;
    [SerializeField] private Plotter plotterPrefab;
    [SerializeField] private Rope ropePrefab;
    [SerializeField] private GameObject shaftPrefab;

    [SerializeField] private OperationGenerator operationGeneratorPrefab;

    public void Clear()
    {
        var list = new List<Transform>();
        foreach (Transform t in transform)
            list.Add(t);

        list.ForEach(t => t.parent = null);
        list.ForEach(t => DestroyImmediate(t.gameObject));
    }

    public void Generate(Node topNode, float xFrom, float xTo, float yFrom, float yTo)
    {
        Clear();

        // generate math function
        var inputNodes = new List<OperationGenerator>();
        var topNodeGenerator = Instantiate(operationGeneratorPrefab, transform);
        topNodeGenerator.Generate(topNode, operationGeneratorPrefab, ref inputNodes);

        // create yGearBox and align topNodeGenerator output to it
        var yGearBox = CreateYGearBox(topNodeGenerator, yFrom, yTo);

        // make xGears and connect them to all inputNodes
        var bounds = OperationGenerator.GetBoundsInGivenSpace(topNodeGenerator.transform.localPosition, transform, topNodeGenerator.LocalSpaceBounds, topNodeGenerator.transform);
        var xGearXPos = bounds.center.x + bounds.extents.x + 15f;
        var xGears = new List<Gear>();
        MeshGenerationManager.Pause();
        foreach (var inputNode in inputNodes)
        {
            var xGear = Instantiate(gearPrefab, transform);
            xGear.transform.localPosition = new Vector3(xGearXPos, 0, transform.InverseTransformPoint(inputNode.OutputGear.transform.position).z);

            MakeBeltConnection(xGear, inputNode.OutputGear);

            xGears.Add(xGear);
        }
        MeshGenerationManager.UnPause();

        // create shaft which joins all xGears
        var orderedXGears = xGears.OrderBy(o => o.transform.localPosition.z);
        if (xGears.Count >= 2)
        {
            AddShaft(xGearXPos, 0, orderedXGears.First().transform.localPosition.z, orderedXGears.Last().transform.localPosition.z);

            foreach (var xGear in orderedXGears.Skip(1))
            {
                xGear.InputComponent = orderedXGears.First();
                xGear.onlyCopyInput = true;
            }
        }

        // create xGearBox and connect it to xGears
        var xGearBox = CreateXGearBox(xFrom, xTo, out Crank inputCrank);
        var firstXGear = orderedXGears.First();
        float zOffset = -3f;
        MoveParentWithChildCoordinates(xGearBox.transform, xGearBox.OutputGear.transform, firstXGear.transform.position + new Vector3(0, 0, zOffset));
        firstXGear.InputComponent = xGearBox.OutputGear;
        firstXGear.onlyCopyInput = true;
        AddShaft(xGearXPos, 0, firstXGear.transform.localPosition.z + zOffset, firstXGear.transform.localPosition.z);

        // add plotter connected to inputCrank and yGearBox output
        AddPlotter(inputCrank, yGearBox.OutputGear, xFrom, xTo, yFrom, yTo);

        Debug.Log("Generated!");
    }

    private OperationGenerator CreateXGearBox(float xFrom, float xTo, out Crank inputCrank)
    {
        var formula = $"-(-x * {(xTo - xFrom) / 10f} + {xFrom})";
        var topNode = MathParser.Parser.Parse(formula);

        var inputNodes = new List<OperationGenerator>();
        var operationGenerator = Instantiate(operationGeneratorPrefab, transform);
        operationGenerator.Generate(topNode, operationGeneratorPrefab, ref inputNodes);

        var inputNodeGenerator = inputNodes.FirstOrDefault();

        inputCrank = Instantiate(crankPrefab, inputNodeGenerator.transform);
        inputCrank.Gear.PlaceOn(inputNodeGenerator.OutputGear, Vector3.right);
        inputNodeGenerator.OutputGear.InputComponent = inputCrank.Gear;

        return operationGenerator;
    }

    private OperationGenerator CreateYGearBox(OperationGenerator topNodeGenerator, float yFrom, float yTo)
    {
        var center = (yFrom + yTo) / 2f;
        var delta = yTo - yFrom;
        var formula = $"(x - {center}) * {1 / (delta) * 5f}"; // * 5f because of plotter gear ratios
        var topNode = MathParser.Parser.Parse(formula);

        var inputNodes = new List<OperationGenerator>();
        var operationGenerator = Instantiate(operationGeneratorPrefab, transform);
        operationGenerator.Generate(topNode, operationGeneratorPrefab, ref inputNodes);

        var operationGeneratorInput = inputNodes.FirstOrDefault();

        operationGeneratorInput.OutputGear.InputComponent = topNodeGenerator.OutputGear;
        operationGeneratorInput.OutputGear.onlyCopyInput = true;
        MoveParentWithChildCoordinates(topNodeGenerator.transform, topNodeGenerator.OutputGear.transform, operationGeneratorInput.OutputGear.transform.position + new Vector3(0, 0, -1f));

        return operationGenerator;
    }

    private void AddPlotter(Crank inputCrank, Gear outputGear, float xFrom, float xTo, float yFrom, float yTo)
    {
        var plotter = Instantiate(plotterPrefab, transform);
        plotter.InputGearY.InputComponent = outputGear;
        plotter.InputGearY.Circumference = outputGear.Circumference;
        plotter.GenerateCoordinateSystem(xFrom, xTo, yFrom, yTo);

        var outputGearEdge = outputGear.GetPositionOfEdge(-transform.right);
        var inputGearYEdge = plotter.InputGearY.GetPositionOfEdge(transform.right);
        var inputGearYPosition = outputGearEdge + plotter.InputGearY.transform.position - inputGearYEdge;

        MoveParentWithChildCoordinates(plotter.transform, plotter.InputGearY.transform, inputGearYPosition);

        var xTransferGear = Instantiate(gearPrefab, transform);
        var inputGearXPosition = plotter.InputGearX.transform.position;
        xTransferGear.SetPositionGlobal(new Vector3(inputGearXPosition.x, inputGearXPosition.y, inputCrank.transform.position.z));
        xTransferGear.Circumference = inputCrank.Gear.Circumference * 2; // so when crank rotates 10 times, the plotter input is rotated 5 times, and plotter is moved by 50 units
        plotter.InputGearX.InputComponent = xTransferGear;
        plotter.InputGearX.onlyCopyInput = true;
        AddShaft(xTransferGear.transform.localPosition.x, xTransferGear.transform.localPosition.y, xTransferGear.transform.localPosition.z, transform.InverseTransformPoint(plotter.InputGearX.transform.position).z);

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

    private void AddShaft(float x, float y, float fromZ, float toZ)
    {
        var shaft = Instantiate(shaftPrefab, transform);

        shaft.transform.localPosition = new Vector3(x, y, (fromZ + toZ) / 2);
        shaft.transform.localScale = new Vector3(1, 1, Mathf.Abs(toZ - fromZ));
    }
}
