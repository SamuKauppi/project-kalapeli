using System.Collections.Generic;
using UnityEngine;

public class GeneratedMesh
{
    List<Vector3> vertices = new();
    List<Vector3> normals = new();
    List<Vector2> uvs = new();
    List<List<int>> submeshIndices = new();

    private readonly float[] uvXBounds = new float[] { float.MaxValue, float.MinValue };
    private readonly float[] uvYBounds = new float[] { float.MaxValue, float.MinValue };

    public List<Vector3> Vertices { get { return vertices; } set { vertices = value; } }
    public List<Vector3> Normals { get { return normals; } set { normals = value; } }
    public List<Vector2> UVs { get { return uvs; } set { uvs = value; } }
    public List<List<int>> SubmeshIndices { get { return submeshIndices; } set { submeshIndices = value; } }

    public void AddTriangle(MeshTriangle _triangle)
    {
        int currentVerticeCount = vertices.Count;

        vertices.AddRange(_triangle.Vertices);
        normals.AddRange(_triangle.Normals);
        uvs.AddRange(_triangle.UVs);

        if (submeshIndices.Count < _triangle.SubmeshIndex + 1)
        {
            for (int i = submeshIndices.Count; i < _triangle.SubmeshIndex + 1; i++)
            {
                submeshIndices.Add(new List<int>());
            }
        }

        for (int i = 0; i < 3; i++)
        {
            submeshIndices[_triangle.SubmeshIndex].Add(currentVerticeCount + i);
        }
    }

    public Mesh GetGeneratedMesh()
    {
        Mesh mesh = new();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(1, uvs);

        mesh.subMeshCount = submeshIndices.Count;
        for (int i = 0; i < submeshIndices.Count; i++)
        {
            mesh.SetTriangles(submeshIndices[i], i);
        }
        return mesh;
    }
    public float GetVolume()
    {
        return VolumeOfMesh();
    }

    private float VolumeOfMesh()
    {
        float vols = 0f;

        for (int i = 0; i < vertices.Count; i += 3)
        {
            if (i + 2 >= vertices.Count)
                break;

            // Add the signed volume of the triangle to the total volume
            vols += SignedVolumeOfTriangle(vertices[i], vertices[i + 1], vertices[i + 2]);
        }

        return Mathf.Abs(vols); // Return the absolute volume
    }


    private float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var v321 = p3.x * p2.y * p1.z;
        var v231 = p2.x * p3.y * p1.z;
        var v312 = p3.x * p1.y * p2.z;
        var v132 = p1.x * p3.y * p2.z;
        var v213 = p2.x * p1.y * p3.z;
        var v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }
}
