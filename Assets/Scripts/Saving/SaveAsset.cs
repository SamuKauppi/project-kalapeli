#if UNITY_EDITOR
// Save object
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;

public static class SaveAsset
{
    public static void SaveGameObjectAsPrefab(GameObject obj)
    {
        if (obj != null)
        {
            if (obj.TryGetComponent<MeshFilter>(out var mf))
            {
                var savePath = "Assets/ExportedMesh.obj";
                SaveMeshAsOBJ(mf.mesh, savePath);
            }
        }
        else
        {
            Debug.LogError("GameObject is null, cannot save.");
        }
    }

    public static void SaveMeshAsOBJ(Mesh mesh, string filePath)
    {
        StringBuilder sb = new();

        // Write the vertices
        foreach (Vector3 v in mesh.vertices)
        {
            sb.AppendFormat("v {0} {1} {2}\n", v.x, v.y, v.z);
        }

        // Write the normals
        foreach (Vector3 n in mesh.normals)
        {
            sb.AppendFormat("vn {0} {1} {2}\n", n.x, n.y, n.z);
        }

        // Write the UVs
        foreach (Vector2 uv in mesh.uv)
        {
            sb.AppendFormat("vt {0} {1}\n", uv.x, uv.y);
        }

        // Write the faces (triangles)
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                            mesh.triangles[i] + 1,    // OBJ format starts indexing from 1
                            mesh.triangles[i + 1] + 1,
                            mesh.triangles[i + 2] + 1);
        }

        // Write to file
        File.WriteAllText(filePath, sb.ToString());

        Debug.Log("Saved mesh");
    }
}

#endif
