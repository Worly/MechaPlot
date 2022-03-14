using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MeshBuilder
{
    private HashSet<Triangle> triangles = new HashSet<Triangle>();


    public MeshBuilder()
    {

    }

    public void AddTriangles(IEnumerable<Triangle> triangles)
    {
        foreach (var triangle in triangles)
            this.triangles.Add(triangle);
    }

    public List<Vertex> GenerateCircle(int vertexCount, float radius, float zValue, int direction)
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

    public List<Triangle> MakeCircleBridge(List<Vertex> circle1, List<Vertex> circle2, bool copyVertices)
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
                quadVertices = quadVertices.Select(o => o.Duplicate()).ToList();

            triangles.AddRange(MakeQuad(quadVertices));
        }

        AddTriangles(triangles);

        return triangles;
    }

    public List<Vertex> GenerateLine(int vertexCount, float length, float xValue, float zValue, int direction)
    {
        var result = new List<Vertex>();

        for (int i = 0; i < vertexCount; i++)
        {
            var yValue = direction * (length / (vertexCount - 1) * i - length / 2f);

            var vertex = new Vertex
            (
                x: xValue,
                y: yValue,
                z: zValue
            );
            result.Add(vertex);
        }

        return result;
    }

    public List<Triangle> MakeLineBridge(List<Vertex> line1, List<Vertex> line2, bool copyVertices)
    {
        if (line1.Count != line2.Count)
            throw new ArgumentException("Line1 vertex count must be same as line2 vertex count");

        var triangles = new List<Triangle>();

        for (int i = 0; i < line1.Count - 1; i++)
        {
            var quadVertices = new List<Vertex>()
            {
                line1[i],
                line1[i + 1],
                line2[i + 1],
                line2[i]
            };

            if (copyVertices)
                quadVertices = quadVertices.Select(o => o.Duplicate()).ToList();

            triangles.AddRange(MakeQuad(quadVertices));
        }

        AddTriangles(triangles);

        return triangles;
    }

    public List<Triangle> MakeQuad(List<Vertex> vertices, bool copyVertices = false)
    {
        if (vertices.Count != 4)
            throw new ArgumentException("Quad must have exactly 4 vertices");

        var triangles = new List<Triangle>();

        if (copyVertices)
            vertices = vertices.Select(o => o.Duplicate()).ToList();

        triangles.Add(new Triangle(vertices[0], vertices[1], vertices[2]));
        triangles.Add(new Triangle(vertices[2], vertices[3], vertices[0]));

        AddTriangles(triangles);

        return triangles;
    }

    public Mesh Build()
    {
        var mesh = new Mesh();

        var vertexSet = new HashSet<Vertex>();

        foreach (var triangle in this.triangles)
        {
            vertexSet.Add(triangle.Vertices[0]);
            vertexSet.Add(triangle.Vertices[1]);
            vertexSet.Add(triangle.Vertices[2]);
        }

        var vertexList = vertexSet.ToList();

        mesh.vertices = vertexList.Select(o => o.Vector3).ToArray();
        mesh.normals = vertexList.Select(o => o.Normal).ToArray();

        mesh.triangles = this.triangles.SelectMany(t => t.Vertices.Select(v => vertexList.IndexOf(v))).ToArray();

        mesh.name = "Generated mesh";
        //mesh.Optimize();
        return mesh;
    }
}

public class Vertex
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

    public Vertex Duplicate()
    {
        return new Vertex(Vector3);
    }
}

public class Triangle
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