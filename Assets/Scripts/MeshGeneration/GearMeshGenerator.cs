using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GearMeshGenerator : MonoBehaviour
{
    [SerializeField]
    public float circumference;

    [SerializeField]
    public float innerCircumference = 2;

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

    [SerializeField]
    public GearType gearType = GearType.Spur;

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

        float outerRadiusLeft = 0, outerRadiusRight = 0;
        if (gearType == GearType.Spur)
            outerRadiusLeft = outerRadiusRight = outerRadius;
        else if (gearType == GearType.BevelLeft)
        {
            outerRadiusLeft = outerRadius - thickness / 2f;
            outerRadiusRight = outerRadius + thickness / 2f;
        }
        else if (gearType == GearType.BevelRight)
        {
            outerRadiusLeft = outerRadius + thickness / 2f;
            outerRadiusRight = outerRadius - thickness / 2f;
        }

        var innerRadius = innerCircumference / (2 * Mathf.PI);

        var circleVertexCount = (int)Mathf.Round(circumference / toothWidth);

        var meshBuilder = new MeshBuilder();

        GenerateSide(meshBuilder, outerRadiusLeft, innerRadius, circleVertexCount, -thickness / 2f, -1, out List<Vertex> outerLeftCircle, out List<Vertex> innerLeftCircle);
        GenerateSide(meshBuilder, outerRadiusRight, innerRadius, circleVertexCount, thickness / 2f, 1, out List<Vertex> outerRightCircle, out List<Vertex> innerRightCircle);

        innerLeftCircle.Reverse(1, innerLeftCircle.Count - 1);
        outerRightCircle.Reverse(1, outerRightCircle.Count - 1);

        meshBuilder.MakeCircleBridge(innerRightCircle, innerLeftCircle, copyVertices: true);

        if (circleVertexCount % 2 == 0)
            GenerateTeeth(meshBuilder, outerRightCircle, outerLeftCircle);
        else
            meshBuilder.MakeCircleBridge(outerRightCircle, outerLeftCircle, copyVertices: true);

        meshFilter.mesh = meshBuilder.Build();
    }

    private void GenerateSide(MeshBuilder meshBuilder, float outerRadius, float innerRadius, int vertexCount, float zValue, int zNormalDirection, out List<Vertex> outerCircle, out List<Vertex> innerCircle)
    {
        outerCircle = meshBuilder.GenerateCircle(vertexCount, outerRadius, zValue, direction: -zNormalDirection);

        innerCircle = meshBuilder.GenerateCircle(vertexCount, innerRadius, zValue, direction: -zNormalDirection);

        meshBuilder.MakeCircleBridge(outerCircle, innerCircle, copyVertices: false);
    }

    private void GenerateTeeth(MeshBuilder meshBuilder, List<Vertex> circle1, List<Vertex> circle2)
    {
        if (circle1.Count != circle2.Count)
            throw new ArgumentException("Circle1 vertex count must be same as circle2 vertex count");

        var newCircle1Vertices = GenerateTeethVertices(meshBuilder, circle1, out List<Triangle> triangles1);
        var newCircle2Vertices = GenerateTeethVertices(meshBuilder, circle2, out _);

        foreach (var triangle in triangles1)
            triangle.Flip();

        meshBuilder.MakeCircleBridge(newCircle1Vertices, newCircle2Vertices, copyVertices: true);
    }

    private List<Vertex> GenerateTeethVertices(MeshBuilder meshBuilder, List<Vertex> circle, out List<Triangle> triangles)
    {
        var newVertices = new List<Vertex>();

        triangles = new List<Triangle>();

        for (int i = 0; i < circle.Count; i++)
        {
            // generate tooth
            if (i % 2 == 0)
            {
                var firstVertex = circle[i];
                var sixthVertex = circle[(i + 1) % circle.Count];

                var pitchingVector = sixthVertex.Vector3 - firstVertex.Vector3;

                newVertices.Add(firstVertex);

                var newPos = firstVertex.Vector3.normalized * toothHeight * toothPitchStartFraction + firstVertex.Vector3;
                var secondVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
                newVertices.Add(secondVertex);

                newPos = firstVertex.Vector3.normalized * toothHeight + firstVertex.Vector3 + pitchingVector * toothPitchFraction;
                var thirdVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
                newVertices.Add(thirdVertex);

                newPos = sixthVertex.Vector3.normalized * toothHeight + sixthVertex.Vector3 - pitchingVector * toothPitchFraction;
                var fourthVector = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
                newVertices.Add(fourthVector);

                newPos = sixthVertex.Vector3.normalized * toothHeight * toothPitchStartFraction + sixthVertex.Vector3;
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

public enum GearType
{
    Spur,
    BevelLeft,
    BevelRight
}