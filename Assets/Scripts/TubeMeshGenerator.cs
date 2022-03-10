using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TubeMeshGenerator : MonoBehaviour
{
    [SerializeField]
    public float circumference = 3;

    [SerializeField]
    public float innerCircumference = 2;

    [SerializeField]
    public float length = 2;

    public void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();

        if (circumference <= innerCircumference)
        {
            Debug.LogError("Circumference must be greater than inner circumference");

            meshFilter.mesh = new Mesh();

            return;
        }

        var outerRadius = circumference / (2 * Mathf.PI);
        var innerRadius = innerCircumference / (2 * Mathf.PI);

        var circleVertexCount = 20;

        var meshBuilder = new MeshBuilder();

        GenerateSide(meshBuilder, outerRadius, innerRadius, circleVertexCount, -length / 2f, -1, out List<Vertex> outerLeftCircle, out List<Vertex> innerLeftCircle);
        GenerateSide(meshBuilder, outerRadius, innerRadius, circleVertexCount, length / 2f, 1, out List<Vertex> outerRightCircle, out List<Vertex> innerRightCircle);

        innerLeftCircle.Reverse(1, innerLeftCircle.Count - 1);
        outerRightCircle.Reverse(1, outerRightCircle.Count - 1);

        meshBuilder.MakeCircleBridge(innerRightCircle, innerLeftCircle, copyVertices: true);
        meshBuilder.MakeCircleBridge(outerRightCircle, outerLeftCircle, copyVertices: true);

        meshFilter.mesh = meshBuilder.Build();
    }

    private void GenerateSide(MeshBuilder meshBuilder, float outerRadius, float innerRadius, int vertexCount, float zValue, int zNormalDirection, out List<Vertex> outerCircle, out List<Vertex> innerCircle)
    {
        outerCircle = meshBuilder.GenerateCircle(vertexCount, outerRadius, zValue, direction: -zNormalDirection);

        innerCircle = meshBuilder.GenerateCircle(vertexCount, innerRadius, zValue, direction: -zNormalDirection);

        meshBuilder.MakeCircleBridge(outerCircle, innerCircle, copyVertices: false);
    }
}
