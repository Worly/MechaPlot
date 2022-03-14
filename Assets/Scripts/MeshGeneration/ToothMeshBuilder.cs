using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ToothMeshBuilder
{
    private static readonly float PITCH_START_FRACTION = 0.5f;
    private static readonly float PITCH_FRACTION = 0.3f;
    private static readonly float WIDEN_FRACTION = 0.2f;

    public static List<Vertex> MakeTooth(this MeshBuilder meshBuilder, Vertex firstVertex, Vertex lastVertex, Vector3 firstToothDirection, Vector3 lastToothDirection, out List<Triangle> triangles, float toothHeight)
    {
        var newVertices = new List<Vertex>();

        triangles = new List<Triangle>();

        var pitchingVector = lastVertex.Vector3 - firstVertex.Vector3;

        newVertices.Add(firstVertex);

        var newPos = firstToothDirection * toothHeight * PITCH_START_FRACTION + firstVertex.Vector3;
        var secondVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
        newVertices.Add(secondVertex);

        newPos = firstToothDirection * toothHeight + firstVertex.Vector3 + pitchingVector * PITCH_FRACTION;
        var thirdVertex = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
        newVertices.Add(thirdVertex);

        newPos = lastToothDirection * toothHeight + lastVertex.Vector3 - pitchingVector * PITCH_FRACTION;
        var fourthVector = new Vertex(newPos.x, newPos.y, firstVertex.Vector3.z);
        newVertices.Add(fourthVector);

        newPos = lastToothDirection * toothHeight * PITCH_START_FRACTION + lastVertex.Vector3;
        var fifthVertex = new Vertex(newPos.x, newPos.y, lastVertex.Vector3.z);
        newVertices.Add(fifthVertex);

        newVertices.Add(lastVertex);

        firstVertex.Vector3 = firstVertex.Vector3 - pitchingVector * WIDEN_FRACTION;
        lastVertex.Vector3 = lastVertex.Vector3 + pitchingVector * WIDEN_FRACTION;

        triangles.AddRange(meshBuilder.MakeQuad(new List<Vertex>() { secondVertex, thirdVertex, fourthVector, fifthVertex }));
        triangles.AddRange(meshBuilder.MakeQuad(new List<Vertex>() { firstVertex, secondVertex, fifthVertex, lastVertex }));

        return newVertices;
    }
}