using MathParser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Gear gearPrefab;
    [SerializeField] private Crank crankPrefab;
    [SerializeField] private Differential differentialPrefab;
    [SerializeField] private Multiplier multiplierPrefab;
    [SerializeField] private ConstantMultiplier constantMultiplierPrefab;
    [SerializeField] private Rope ropePrefab;
    [SerializeField] private GameObject shaftPrefab;

    public Gear OutputGear { get; private set; }
    public Bounds LocalSpaceBounds { get; private set; }

    public void Generate(Node node, OperationGenerator operationGeneratorPrefab, ref List<OperationGenerator> inputNodes)
    {
        this.gameObject.name = "Operation: " + node.ToString();

        if (node is InputNode)
        {
            OutputGear = Instantiate(gearPrefab, transform);
            OutputGear.transform.localPosition = Vector3.zero;
            inputNodes.Add(this);
        }
        else if (node is ValueNode valueNode)
        {
            OutputGear = Instantiate(gearPrefab, transform);
            OutputGear.transform.localPosition = Vector3.zero;
            OutputGear.Value = valueNode.Value;
        }
        else if (node is OperationNode operationNode)
        {
            if (operationNode.Operation == Operation.MULTIPLICATION && operationNode.IsConstantOperation(out float constant, out Node variableNode))
            {
                var nodeGenerator = Instantiate(operationGeneratorPrefab, transform);
                nodeGenerator.Generate(variableNode, operationGeneratorPrefab, ref inputNodes);
                GenerateConstantMultiplier(constant, nodeGenerator);
            }
            else
            {
                var leftOperator = Instantiate(operationGeneratorPrefab, transform);
                leftOperator.Generate(operationNode.Left, operationGeneratorPrefab, ref inputNodes);
            
                var rightOperator = Instantiate(operationGeneratorPrefab, transform);
                rightOperator.Generate(operationNode.Right, operationGeneratorPrefab, ref inputNodes);

                Gear leftGear;
                Gear rightGear;

                if (operationNode.Operation == Operation.ADDITION)
                {
                    var differential = Instantiate(differentialPrefab, transform);
                    differential.transform.localPosition = Vector3.zero;
                    leftGear = differential.InputGear1;
                    rightGear = differential.InputGear2;
                    OutputGear = differential.OutputGear;
                }
                else if (operationNode.Operation == Operation.MULTIPLICATION)
                {
                    var multiplier = Instantiate(multiplierPrefab, transform);
                    multiplier.transform.localPosition = Vector3.zero;
                    leftGear = multiplier.InputGear1;
                    rightGear = multiplier.InputGear2;
                    OutputGear = multiplier.OutputGear;
                }
                else
                    throw new System.NotImplementedException("Operation is not yet supported!");

                leftGear.InputComponent = leftOperator.OutputGear;
                leftGear.Circumference = leftOperator.OutputGear.Circumference;
                MoveChildForGearMeshing(leftGear, leftOperator.transform, leftOperator.OutputGear);

                rightGear.InputComponent = rightOperator.OutputGear;
                rightGear.Circumference = rightOperator.OutputGear.Circumference;
                MoveChildForGearMeshing(rightGear, rightOperator.transform, rightOperator.OutputGear);

                var leftBounds = GetBoundsInGivenSpace(leftOperator.transform.localPosition, transform, leftOperator.LocalSpaceBounds, leftOperator.transform);
                var rightBounds = GetBoundsInGivenSpace(rightOperator.transform.localPosition, transform, rightOperator.LocalSpaceBounds, rightOperator.transform);

                if (leftBounds.Intersects(rightBounds))
                {
                    var intersectWidth = (leftBounds.center.z + leftBounds.extents.z) - (rightBounds.center.z - rightBounds.extents.z);
                    // give some breathing space
                    intersectWidth += 3f;

                    var leftOperatorOffset = new Vector3(0, 0, -intersectWidth);

                    leftOperator.transform.localPosition = leftOperator.transform.localPosition + leftOperatorOffset;

                    var transferGear = Instantiate(gearPrefab, transform);
                    transferGear.transform.position = leftGear.transform.position + leftOperatorOffset;
                    transferGear.InputComponent = leftOperator.OutputGear;
                    transferGear.Circumference = leftOperator.OutputGear.Circumference;

                    leftGear.InputComponent = transferGear;
                    leftGear.OnlyCopyInput = true;

                    AddShaft(
                        transferGear.transform.localPosition.x,
                        transferGear.transform.localPosition.y,
                        transferGear.transform.localPosition.z,
                        transform.InverseTransformPoint(leftGear.transform.position).z);
                }
            }
        }
        else if (node is UnaryOperationNode)
            throw new System.ArgumentException("OperationGenerator cannot generate UnaryOperationNodes, transform it to OperationNode first!");

        RecalculateBounds();
    }

    private void GenerateConstantMultiplier(float constant, OperationGenerator nodeGenerator)
    {
        var constantMultiplier = Instantiate(constantMultiplierPrefab, transform);
        constantMultiplier.transform.localPosition = Vector3.zero;
        constantMultiplier.GenerateGears(constant);

        constantMultiplier.InputGear.InputComponent = nodeGenerator.OutputGear;
        constantMultiplier.InputGear.Circumference = nodeGenerator.OutputGear.Circumference;

        MoveChildForGearMeshing(constantMultiplier.InputGear, nodeGenerator.transform, nodeGenerator.OutputGear);

        this.OutputGear = constantMultiplier.OutputGear;
    }

    public static void MoveChildForGearMeshing(Gear parentGear, Transform childTransform, Gear childGear)
    {
        var parentGearEdge = parentGear.GetPositionOfEdge(Vector3.right);
        var childGearEdge = childGear.GetPositionOfEdge(-Vector3.right);

        childTransform.position = parentGearEdge - childTransform.InverseTransformPoint(childGearEdge);
    }

    private void AddShaft(float x, float y, float fromZ, float toZ)
    {
        var shaft = Instantiate(shaftPrefab, transform);

        shaft.transform.localPosition = new Vector3(x, y, (fromZ + toZ) / 2);
        shaft.transform.localScale = new Vector3(1, 1, Mathf.Abs(toZ - fromZ));
    }

    #region BoundsCalculation

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.TransformPoint(LocalSpaceBounds.center), LocalSpaceBounds.size);
    }

    private void RecalculateBounds()
    {
        var newBounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (Transform child in transform)
        {
            Bounds bounds = new Bounds(child.localPosition, Vector3.zero);
            if (child.TryGetComponent(out OperationGenerator operationGenerator))
                bounds = GetBoundsInGivenSpace(bounds.center, transform, operationGenerator.LocalSpaceBounds, operationGenerator.transform);
            else
                GetLocalSpaceBoundsOfObject(child, ref bounds);

            newBounds.Encapsulate(bounds);
        }

        this.LocalSpaceBounds = newBounds;
    }

    private void GetLocalSpaceBoundsOfObject(Transform child, ref Bounds bounds)
    {
        var mesh = child.GetComponent<MeshFilter>();
        if (mesh)
        {
            var lsBounds = mesh.mesh.bounds;
            bounds.Encapsulate(GetBoundsInGivenSpace(bounds.center, transform, lsBounds, child));
        }
        foreach (Transform grandChild in child.transform)
        {
            GetLocalSpaceBoundsOfObject(grandChild, ref bounds);
        }
    }

    public static Bounds GetBoundsInGivenSpace(Vector3 startingCenter, Transform givenSpace, Bounds bounds, Transform boundsLocalSpace)
    {
        var newBounds = new Bounds(startingCenter, Vector3.zero);

        var wsMin = boundsLocalSpace.TransformPoint(bounds.min);
        var wsMax = boundsLocalSpace.TransformPoint(bounds.max);

        newBounds.Encapsulate(givenSpace.InverseTransformPoint(wsMin));
        newBounds.Encapsulate(givenSpace.InverseTransformPoint(wsMax));

        return newBounds;
    }

    #endregion
}