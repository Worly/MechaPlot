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

        var mesh = new Mesh();

        var circleVertexCount = (int)Mathf.Round(circumference / toothWidth);

        var leftSideTriangles = GenerateSide(outerRadius, innerRadius, circleVertexCount, -thickness / 2f, -1, out List<Vertex> outerLeftCircle, out List<Vertex> innerLeftCircle);
        var rightSideTriangles = GenerateSide(outerRadius, innerRadius, circleVertexCount, thickness / 2f, 1, out List<Vertex> outerRightCircle, out List<Vertex> innerRightCircle);

        innerLeftCircle.Reverse(1, innerLeftCircle.Count - 1);
        outerRightCircle.Reverse(1, outerRightCircle.Count - 1);

        var innerBridgeTriangles = GenerateCircleBridge(innerRightCircle, innerLeftCircle, copyVertices: true);

        List<Triangle> outerBridgeTriangles;
        if (circleVertexCount % 2 == 0)
            outerBridgeTriangles = GenerateTeeth(outerRightCircle, outerLeftCircle);
        else
            outerBridgeTriangles = GenerateCircleBridge(outerRightCircle, outerLeftCircle, copyVertices: true);

        var meshBuilder = new MeshBuilder();
        meshBuilder
            .AddTriangles(leftSideTriangles)
            .AddTriangles(rightSideTriangles)
            .AddTriangles(innerBridgeTriangles)
            .AddTriangles(outerBridgeTriangles)
            .Build(out Vector3[] vertices, out int[] triangles, out Vector3[] normals);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;

        meshFilter.mesh = mesh;
    }

    private List<Triangle> GenerateSide(float outerRadius, float innerRadius, int vertexCount, float zValue, int zNormalDirection, out List<Vertex> outerCircle, out List<Vertex> innerCircle)
    {
        outerCircle = GenerateCircle(vertexCount, outerRadius, zValue, direction: -zNormalDirection);

        innerCircle = GenerateCircle(vertexCount, innerRadius, zValue, direction: -zNormalDirection);

        return GenerateCircleBridge(outerCircle, innerCircle, copyVertices: false);
    }

    private List<Vertex> GenerateCircle(int vertexCount, float radius, float zValue, int direction)
    {
        var result = new List<Vertex>();

        for (int i = 0; i < vertexCount; i++)
        {
            var angle = direction * 2 * Mathf.PI * i / vertexCount;

            var vertex = new Vertex
            (
                x: Mathf.Sin(angle) * radius,
                y: Mathf.Cos(angle) * radius,
                z: zValue
            );
            result.Add(vertex);
        }

        return result;
    }

    private List<Triangle> GenerateCircleBridge(List<Vertex> circle1, List<Vertex> circle2, bool copyVertices)
    {
        if (circle1.Count != circle2.Count)
            throw new ArgumentException("Circle1 vertex count must be same as circle2 vertex count");

        var triangles = new List<Triangle>();

        for (int i = 0; i < circle1.Count; i++)
        {
            var quadVertices = new List<Vertex>()
            {
                circle1[i],
                circle1[(i + 1) % circle1.Count],
                circle2[(i + 1) % circle1.Count],
                circle2[i]
            };

            if (copyVertices)
                quadVertices = quadVertices.Select(o => new Vertex(o.Vector3)).ToList();

            triangles.AddRange(MakeQuad(quadVertices));
        }

        return triangles;
    }

    private List<Triangle> GenerateTeeth(List<Vertex> circle1, List<Vertex> circle2)
    {
        if (circle1.Count != circle2.Count)
            throw new ArgumentException("Circle1 vertex count must be same as circle2 vertex count");

        var triangles = new List<Triangle>();

        var newCircle1Vertices = GenerateTeethVertices(circle1, out List<Triangle> triangles1);
        var newCircle2Vertices = GenerateTeethVertices(circle2, out List<Triangle> triangles2);

        foreach (var triangle in triangles1)
            triangle.Flip();

        triangles.AddRange(triangles1);
        triangles.AddRange(triangles2);

        triangles.AddRange(GenerateCircleBridge(newCircle1Vertices, newCircle2Vertices, copyVertices: true));

        return triangles;
    }

    private List<Vertex> GenerateTeethVertices(List<Vertex> circle, out List<Triangle> triangles)
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

                triangles.AddRange(MakeQuad(new List<Vertex>() { secondVertex, thirdVertex, fourthVector, fifthVertex }));
                triangles.AddRange(MakeQuad(new List<Vertex>() { firstVertex, secondVertex, fifthVertex, sixthVertex }));
            }
        }

        return newVertices;
    }

    private List<Triangle> MakeQuad(List<Vertex> vertices)
    {
        if (vertices.Count != 4)
            throw new ArgumentException("Quad must have exactly 4 vertices");

        var triangles = new List<Triangle>();

        triangles.Add(new Triangle(vertices[0], vertices[1], vertices[2]));
        triangles.Add(new Triangle(vertices[2], vertices[3], vertices[0]));

        return triangles;
    }

    private class Vertex
    {
        public Vector3 Vector3 { get; set; }
        public Vector3 Normal { get; set; } = new Vector3(0, 0, 0);

        public Vertex(Vector3 vector3)
        {
            Vector3 = vector3;
        }

        public Vertex(float x, float y, float z)
        {
            Vector3 = new Vector3(x, y, z);
        }
    }

    private class Triangle
    {
        public Vertex[] Vertices { get; set; } = new Vertex[3];

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            Vertices[0] = v0;
            Vertices[1] = v1;
            Vertices[2] = v2;

            GenerateNormals();
        }

        public void Flip()
        {
            var temp = Vertices[2];
            Vertices[2] = Vertices[0];
            Vertices[0] = temp;

            GenerateNormals();
        }

        private void GenerateNormals()
        {
            var vector1 = Vertices[1].Vector3 - Vertices[0].Vector3;
            var vector2 = Vertices[2].Vector3 - Vertices[0].Vector3;

            var normal = Vector3.Cross(vector1, vector2);

            Vertices[0].Normal = Vertices[1].Normal = Vertices[2].Normal = normal;
        }
    }

    private class MeshBuilder
    {
        public HashSet<Triangle> triangles = new HashSet<Triangle>();

        public MeshBuilder AddTriangles(IEnumerable<Triangle> triangles)
        {
            foreach (var triangle in triangles)
                this.triangles.Add(triangle);

            return this;
        }

        public void Build(out Vector3[] vertices, out int[] triangles, out Vector3[] normals)
        {
            var vertexSet = new HashSet<Vertex>();

            foreach (var triangle in this.triangles)
            {
                vertexSet.Add(triangle.Vertices[0]);
                vertexSet.Add(triangle.Vertices[1]);
                vertexSet.Add(triangle.Vertices[2]);
            }

            var vertexList = vertexSet.ToList();

            vertices = vertexList.Select(o => o.Vector3).ToArray();
            normals = vertexList.Select(o => o.Normal).ToArray();

            triangles = this.triangles.SelectMany(t => t.Vertices.Select(v => vertexList.IndexOf(v))).ToArray();
        }
    }
}
