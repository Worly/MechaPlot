using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RackMeshGenerator : MonoBehaviour, IMeshGenerator
{
    [SerializeField]
    public float length = 10f;

    [SerializeField]
    public float width = 2f;

    [SerializeField]
    public float toothWidth = 0.5f;

    [SerializeField]
    public float toothHeight = 0.8f;

    [SerializeField]
    public float thickness = 1;

    public void Generate()
    {
        MeshGenerationManager.GenerateOrQueue(this);
    }

    public void GenerateMeshInternal()
    {
        var meshFilter = GetComponent<MeshFilter>();

        var vertexCount = (int)Mathf.Round(length / toothWidth) + 1;

        var meshBuilder = new MeshBuilder();

        GenerateSide(meshBuilder, length, vertexCount, -thickness / 2f, 1, out List<Vertex> outerLeftEdge, out List<Vertex> innerLeftEdge);
        GenerateSide(meshBuilder, length, vertexCount, thickness / 2f, -1, out List<Vertex> outerRightEdge, out List<Vertex> innerRightEdge);

        innerLeftEdge.Reverse();
        outerRightEdge.Reverse();

        meshBuilder.MakeLineBridge(innerRightEdge, innerLeftEdge, copyVertices: true);

        GenerateTeeth(meshBuilder, outerRightEdge, outerLeftEdge);

        meshBuilder.MakeQuad(new List<Vertex>() { innerLeftEdge.First(), outerLeftEdge.Last(), outerRightEdge.Last(), innerRightEdge.First() }, true);
        meshBuilder.MakeQuad(new List<Vertex>() { innerRightEdge.Last(), outerRightEdge.First(), outerLeftEdge.First(), innerLeftEdge.Last() }, true);

        meshFilter.mesh = meshBuilder.Build();
        meshFilter.mesh.RecalculateBounds();
    }

    private void GenerateSide(MeshBuilder meshBuilder, float length, int vertexCount, float zValue, int zNormalDirection, out List<Vertex> outerEdge, out List<Vertex> innerEdge)
    {
        outerEdge = meshBuilder.GenerateLine(vertexCount, length, width / 2f, zValue, direction: -zNormalDirection);

        innerEdge = meshBuilder.GenerateLine(vertexCount, length, -width / 2f, zValue, direction: -zNormalDirection);

        meshBuilder.MakeLineBridge(outerEdge, innerEdge, copyVertices: false);
    }

    private void GenerateTeeth(MeshBuilder meshBuilder, List<Vertex> line1, List<Vertex> line2)
    {
        if (line1.Count != line2.Count)
            throw new ArgumentException("Line1 vertex count must be same as line2 vertex count");

        var newCircle1Vertices = GenerateTeethVertices(meshBuilder, line1, out List<Triangle> triangles1);
        var newCircle2Vertices = GenerateTeethVertices(meshBuilder, line2, out _);

        foreach (var triangle in triangles1)
            triangle.Flip();

        meshBuilder.MakeLineBridge(newCircle1Vertices, newCircle2Vertices, copyVertices: true);
    }

    private List<Vertex> GenerateTeethVertices(MeshBuilder meshBuilder, List<Vertex> line, out List<Triangle> triangles)
    {
        var newVertices = new List<Vertex>();

        triangles = new List<Triangle>();

        var toothDirection = new Vector3(1, 0, 0);

        for (int i = 0; i < line.Count - 1; i++)
        {
            // generate tooth
            if (i % 2 == 0)
            {
                var firstVertex = line[i];
                var lastVertex = line[i + 1];

                var toothVertices = meshBuilder.MakeTooth(firstVertex, lastVertex, toothDirection, toothDirection, out List<Triangle> toothTriangles, toothHeight);

                newVertices.AddRange(toothVertices);
                triangles.AddRange(toothTriangles);
            }
        }

        if (line.Count % 2 == 1)
            newVertices.Add(line.Last());

        return newVertices;
    }
}