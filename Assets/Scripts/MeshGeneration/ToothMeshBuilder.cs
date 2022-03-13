using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ToothMeshBuilder
{
    public static List<Vertex> MakeTooth(this MeshBuilder meshBuilder, Vertex firstVertex, Vertex lastVertex, Vector3 firstToothDirection, Vector3 lastToothDirection, out List<Triangle> triangles, float toothHeight, float toothPitchStartFraction, float toothPitchFraction)
    {
        var newVertices = new List<Vertex>();

        triangles = new List<Triangle>();

        var pitchingVector = lastVertex.Vector3 - firstVertex.Vector3;

        newVertices.Add(firstVertex);

        var newPos = firstToothDirection * toothHeight * toothPitchStartFraction + firstVertex.Vector3;
        var secondVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
        newVertices.Add(secondVertex);

        newPos = firstToothDirection * toothHeight + firstVertex.Vector3 + pitchingVector * toothPitchFraction;
        var thirdVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
        newVertices.Add(thirdVertex);

        newPos = lastToothDirection * toothHeight + lastVertex.Vector3 - pitchingVector * toothPitchFraction;
        var fourthVector = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
        newVertices.Add(fourthVector);

        newPos = lastToothDirection * toothHeight * toothPitchStartFraction + lastVertex.Vector3;
        var fifthVertex = new Vertex(newPos.x, newPos.y, lastVertex.Vector3.z);
        newVertices.Add(fifthVertex);

        newVertices.Add(lastVertex);

        triangles.AddRange(meshBuilder.MakeQuad(new List<Vertex>() { secondVertex, thirdVertex, fourthVector, fifthVertex }));
        triangles.AddRange(meshBuilder.MakeQuad(new List<Vertex>() { firstVertex, secondVertex, fifthVertex, lastVertex }));

        return newVertices;
    }
}