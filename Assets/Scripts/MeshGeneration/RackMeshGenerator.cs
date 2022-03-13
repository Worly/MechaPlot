using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RackMeshGenerator : MonoBehaviour
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
    public float toothPitchStartFraction = 0.5f;

    [SerializeField]
    public float toothPitchFraction = 0.2f;

    [SerializeField]
    public float thickness = 1;

    public void GenerateMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();

        var vertexCount = (int)Mathf.Round(length / toothWidth);

        var meshBuilder = new MeshBuilder();

        GenerateSide(meshBuilder, length, vertexCount, -thickness / 2f, 1, out List<Vertex> outerLeftEdge, out List<Vertex> innerLeftEdge);
        GenerateSide(meshBuilder, length, vertexCount, thickness / 2f, -1, out List<Vertex> outerRightEdge, out List<Vertex> innerRightEdge);

        innerLeftEdge.Reverse();
        outerRightEdge.Reverse();

        meshBuilder.MakeLineBridge(innerRightEdge, innerLeftEdge, copyVertices: true);

        meshBuilder.MakeQuad(new List<Vertex>() { innerLeftEdge.First(), outerLeftEdge.Last(), outerRightEdge.Last(), innerRightEdge.First() }, true);
        meshBuilder.MakeQuad(new List<Vertex>() { innerRightEdge.Last(), outerRightEdge.First(), outerLeftEdge.First(), innerLeftEdge.Last() }, true);

        if (vertexCount % 2 == 0)
            GenerateTeeth(meshBuilder, outerRightEdge, outerLeftEdge);
        else
            Debug.LogError("length / toothWidth must be divisible by 2 to generate teeth");

        meshFilter.mesh = meshBuilder.Build();
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

    private List<Vertex> GenerateTeethVertices(MeshBuilder meshBuilder, List<Vertex> circle, out List<Triangle> triangles)
    {
        var newVertices = new List<Vertex>();

        triangles = new List<Triangle>();

        var toothDirection = new Vector3(1, 0, 0);

        for (int i = 0; i < circle.Count; i++)
        {
            // generate tooth
            if (i % 2 == 0)
            {
                var firstVertex = circle[i];
                var sixthVertex = circle[(i + 1) % circle.Count];

                var pitchingVector = sixthVertex.Vector3 - firstVertex.Vector3;

                newVertices.Add(firstVertex);

                var newPos = toothDirection * toothHeight * toothPitchStartFraction + firstVertex.Vector3;
                var secondVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
                newVertices.Add(secondVertex);

                newPos = toothDirection * toothHeight + firstVertex.Vector3 + pitchingVector * toothPitchFraction;
                var thirdVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
                newVertices.Add(thirdVertex);

                newPos = toothDirection * toothHeight + sixthVertex.Vector3 - pitchingVector * toothPitchFraction;
                var fourthVector = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
                newVertices.Add(fourthVector);

                newPos = toothDirection * toothHeight * toothPitchStartFraction + sixthVertex.Vector3;
                var fifthVertex = new Vertex(newPos.x, newPos.y, sixthVertex.Vector3.z);
                newVertices.Add(fifthVertex);

                newVertices.Add(sixthVertex);

                triangles.AddRange(meshBuilder.MakeQuad(new List<Vertex>() { secondVertex, thirdVertex, fourthVector, fifthVertex }));
                triangles.AddRange(meshBuilder.MakeQuad(new List<Vertex>() { firstVertex, secondVertex, fifthVertex, sixthVertex }));
            }
        }

        return newVertices;
    }
}