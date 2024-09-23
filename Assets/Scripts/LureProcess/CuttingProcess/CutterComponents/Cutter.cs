using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Cutter : MonoBehaviour
{
    private static bool isBusy;
    private static Mesh originalMesh;

    /// <summary>
    /// Cuts Gameobject into two sperate objects
    /// </summary>
    /// <param name="originalGameObject">Object to be cut</param>
    /// <param name="contactPoint">Middle of the cut point</param>
    /// <param name="cutNormal">normal of the cutting plane</param>
    public static void Cut(GameObject originalGameObject, Vector3 contactPoint, Vector3 cutNormal)
    {
        if (isBusy)
            return;

        isBusy = true;

        // Create cut plane used to separate meshes
        Plane cutPlane = new(originalGameObject.transform.InverseTransformDirection(-cutNormal), originalGameObject.transform.InverseTransformPoint(contactPoint));
        originalMesh = originalGameObject.GetComponent<MeshFilter>().mesh;

        // Confirm there is a mesh
        if (originalMesh == null)
        {
            Debug.LogError("Need mesh to cut");
            return;
        }

        List<Vector3> addedVertices = new();

        // Generate left and right mesh
        GeneratedMesh leftMesh = new();
        GeneratedMesh rightMesh = new();

        // Separate meshes and fill the cut
        SeparateMeshes(leftMesh, rightMesh, cutPlane, addedVertices);
        FillCut(addedVertices, cutPlane, leftMesh, rightMesh);

        // Getting and destroying all original colliders to prevent having multiple colliders
        // of different kinds on one object
        var originalCols = originalGameObject.GetComponents<Collider>();
        var physicsMat = originalGameObject.GetComponent<Collider>().material;
        foreach (var col in originalCols)
        {
            Destroy(col);
        }

        // Calculate the volume of the generated mesh
        float leftVol = leftMesh.GetVolume();
        float rightVol = rightMesh.GetVolume();

        // Determine bigger and smaller mesh and create the final meshes based on them
        Mesh biggerMesh = leftVol >= rightVol ? leftMesh.GetGeneratedMesh() : rightMesh.GetGeneratedMesh();
        Mesh smallerMesh = leftVol < rightVol ? leftMesh.GetGeneratedMesh() : rightMesh.GetGeneratedMesh();

        // Assgin the bigger mesh to original object and clean up empty submeshes
        Material mat = originalGameObject.GetComponent<MeshRenderer>().material;
        CleanupSubmeshes(biggerMesh,
            originalGameObject.GetComponent<MeshRenderer>(),
            originalGameObject.GetComponent<MeshFilter>(),
            mat);
        RecalculateUvs(biggerMesh, originalGameObject);

        // Original object
        // Set the collider 
        var collider = originalGameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = biggerMesh;
        collider.convex = true;
        collider.material = physicsMat;

        // New Object separated from original
        GameObject otherObj = new()
        {
            tag = "Piece"
        };

        otherObj.AddComponent<DestroyPiece>();

        // Set transforms
        otherObj.transform.SetPositionAndRotation(originalGameObject.transform.position + (Vector3.up * .05f),
                                               originalGameObject.transform.rotation);
        otherObj.transform.localScale = originalGameObject.transform.localScale;

        // Assgin the smaller mesh to the new object and clean up empty submeshes
        CleanupSubmeshes(smallerMesh,
            otherObj.AddComponent<MeshRenderer>(),
            otherObj.AddComponent<MeshFilter>(),
            mat);

        // Set collider
        otherObj.AddComponent<MeshCollider>().sharedMesh = smallerMesh;
        var cols = otherObj.GetComponents<MeshCollider>();
        foreach (var col in cols)
        {
            col.convex = true;
            col.material = physicsMat;
        }

        // Set rigidbody
        var rightRigidbody = otherObj.AddComponent<Rigidbody>();
        float direction = leftVol > rightVol ? -1f : 1f;
        rightRigidbody.AddRelativeForce(1000f * direction * cutPlane.normal);

        // Free cutter
        isBusy = false;
    }

    /// <summary>
    /// Iterates over all the triangles of all the submeshes of the original mesh to separate the left
    /// and right side of the plane into individual meshes.
    /// </summary>
    /// <param name="leftMesh"></param>
    /// <param name="rightMesh"></param>
    /// <param name="plane"></param>
    /// <param name="addedVertices"></param>
    private static void SeparateMeshes(GeneratedMesh leftMesh, GeneratedMesh rightMesh, Plane plane, List<Vector3> addedVertices)
    {
        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            var subMeshIndices = originalMesh.GetTriangles(i);

            //We are now going through the submesh indices as triangles to determine on what side of the mesh they are.
            for (int j = 0; j < subMeshIndices.Length; j += 3)
            {
                var triangleIndexA = subMeshIndices[j];
                var triangleIndexB = subMeshIndices[j + 1];
                var triangleIndexC = subMeshIndices[j + 2];

                MeshTriangle currentTriangle = GetTriangle(triangleIndexA, triangleIndexB, triangleIndexC, i);

                //We are now using the plane.getside function to see on which side of the cut our trianle is situated 
                //or if it might be cut through
                bool triangleALeftSide = plane.GetSide(originalMesh.vertices[triangleIndexA]);
                bool triangleBLeftSide = plane.GetSide(originalMesh.vertices[triangleIndexB]);
                bool triangleCLeftSide = plane.GetSide(originalMesh.vertices[triangleIndexC]);

                switch (triangleALeftSide)
                {
                    //All three vertices are on the left side of the plane, so they need to be added to the left
                    //mesh
                    case true when triangleBLeftSide && triangleCLeftSide:
                        leftMesh.AddTriangle(currentTriangle);
                        break;
                    //All three vertices are on the right side of the mesh.
                    case false when !triangleBLeftSide && !triangleCLeftSide:
                        rightMesh.AddTriangle(currentTriangle);
                        break;
                    default:
                        CutTriangle(plane, currentTriangle, triangleALeftSide, triangleBLeftSide, triangleCLeftSide, leftMesh, rightMesh, addedVertices);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Returns the tree vertices of a triangle as one MeshTriangle to keep code more readable
    /// </summary>
    /// <param name="_triangleIndexA"></param>
    /// <param name="_triangleIndexB"></param>
    /// <param name="_triangleIndexC"></param>
    /// <param name="_submeshIndex"></param>
    /// <returns></returns>
    private static MeshTriangle GetTriangle(int _triangleIndexA, int _triangleIndexB, int _triangleIndexC, int _submeshIndex)
    {
        //Adding the Vertices at the triangleIndex
        Vector3[] verticesToAdd = {
            originalMesh.vertices[_triangleIndexA],
            originalMesh.vertices[_triangleIndexB],
            originalMesh.vertices[_triangleIndexC]
        };

        //Adding the normals at the triangle index
        Vector3[] normalsToAdd = {
            originalMesh.normals[_triangleIndexA],
            originalMesh.normals[_triangleIndexB],
            originalMesh.normals[_triangleIndexC]
        };

        //adding the uvs at the triangleIndex
        Vector2[] uvsToAdd = {
            originalMesh.uv[_triangleIndexA],
            originalMesh.uv[_triangleIndexB],
            originalMesh.uv[_triangleIndexC]
        };

        return new MeshTriangle(verticesToAdd, normalsToAdd, uvsToAdd, _submeshIndex);
    }

    /// <summary>
    /// Cuts a triangle that exists between both sides of the cut apart adding additional vertices
    /// where needed to create intact triangles on both sides.
    /// </summary>
    /// <param name="plane"></param>
    /// <param name="triangle"></param>
    /// <param name="triangleALeftSide"></param>
    /// <param name="triangleBLeftSide"></param>
    /// <param name="triangleCLeftSide"></param>
    /// <param name="leftMesh"></param>
    /// <param name="rightMesh"></param>
    /// <param name="addedVertices"></param>
    private static void CutTriangle(Plane plane,
                                    MeshTriangle triangle,
                                    bool triangleALeftSide,
                                    bool triangleBLeftSide,
                                    bool triangleCLeftSide,
                                    GeneratedMesh leftMesh,
                                    GeneratedMesh rightMesh,
                                    List<Vector3> addedVertices)
    {
        List<bool> leftSide = new()
        {
            triangleALeftSide,
            triangleBLeftSide,
            triangleCLeftSide
        };

        MeshTriangle leftMeshTriangle = new(new Vector3[2], new Vector3[2], new Vector2[2], triangle.SubmeshIndex);
        MeshTriangle rightMeshTriangle = new(new Vector3[2], new Vector3[2], new Vector2[2], triangle.SubmeshIndex);

        bool left = false;
        bool right = false;

        for (int i = 0; i < 3; i++)
        {
            if (leftSide[i])
            {
                if (!left)
                {
                    left = true;

                    leftMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    leftMeshTriangle.Vertices[1] = leftMeshTriangle.Vertices[0];

                    leftMeshTriangle.UVs[0] = triangle.UVs[i];
                    leftMeshTriangle.UVs[1] = leftMeshTriangle.UVs[0];

                    leftMeshTriangle.Normals[0] = triangle.Normals[i];
                    leftMeshTriangle.Normals[1] = leftMeshTriangle.Normals[0];
                }
                else
                {
                    leftMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    leftMeshTriangle.Normals[1] = triangle.Normals[i];
                    leftMeshTriangle.UVs[1] = triangle.UVs[i];
                }
            }
            else
            {
                if (!right)
                {
                    right = true;

                    rightMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    rightMeshTriangle.Vertices[1] = rightMeshTriangle.Vertices[0];

                    rightMeshTriangle.UVs[0] = triangle.UVs[i];
                    rightMeshTriangle.UVs[1] = rightMeshTriangle.UVs[0];

                    rightMeshTriangle.Normals[0] = triangle.Normals[i];
                    rightMeshTriangle.Normals[1] = rightMeshTriangle.Normals[0];

                }
                else
                {
                    rightMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    rightMeshTriangle.Normals[1] = triangle.Normals[i];
                    rightMeshTriangle.UVs[1] = triangle.UVs[i];
                }
            }
        }

        float normalizedDistance;
        plane.Raycast(
            new Ray(leftMeshTriangle.Vertices[0],
            (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).normalized),
            out float distance);

        normalizedDistance = distance / (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).magnitude;
        Vector3 vertLeft = Vector3.Lerp(leftMeshTriangle.Vertices[0], rightMeshTriangle.Vertices[0], normalizedDistance);
        addedVertices.Add(vertLeft);

        Vector3 normalLeft = Vector3.Lerp(leftMeshTriangle.Normals[0], rightMeshTriangle.Normals[0], normalizedDistance);
        Vector2 uvLeft = Vector2.Lerp(leftMeshTriangle.UVs[0], rightMeshTriangle.UVs[0], normalizedDistance);

        plane.Raycast(new Ray(leftMeshTriangle.Vertices[1], (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).normalized), out distance);

        normalizedDistance = distance / (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).magnitude;
        Vector3 vertRight = Vector3.Lerp(leftMeshTriangle.Vertices[1], rightMeshTriangle.Vertices[1], normalizedDistance);
        addedVertices.Add(vertRight);

        Vector3 normalRight = Vector3.Lerp(leftMeshTriangle.Normals[1], rightMeshTriangle.Normals[1], normalizedDistance);
        Vector2 uvRight = Vector2.Lerp(leftMeshTriangle.UVs[1], rightMeshTriangle.UVs[1], normalizedDistance);

        //TESTING OUR FIRST TRIANGLE
        MeshTriangle currentTriangle;
        Vector3[] updatedVertices = { leftMeshTriangle.Vertices[0], vertLeft, vertRight };
        Vector3[] updatedNormals = { leftMeshTriangle.Normals[0], normalLeft, normalRight };
        Vector2[] updatedUVs = { leftMeshTriangle.UVs[0], uvLeft, uvRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);

        //If our vertices ant the same
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        //SECOND TRIANGLE 
        updatedVertices = new Vector3[] { leftMeshTriangle.Vertices[0], leftMeshTriangle.Vertices[1], vertRight };
        updatedNormals = new Vector3[] { leftMeshTriangle.Normals[0], leftMeshTriangle.Normals[1], normalRight };
        updatedUVs = new Vector2[] { leftMeshTriangle.UVs[0], leftMeshTriangle.UVs[1], uvRight };


        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        //If our vertices arent the same
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        //THIRD TRIANGLE 
        updatedVertices = new Vector3[] { rightMeshTriangle.Vertices[0], vertLeft, vertRight };
        updatedNormals = new Vector3[] { rightMeshTriangle.Normals[0], normalLeft, normalRight };
        updatedUVs = new Vector2[] { rightMeshTriangle.UVs[0], uvLeft, uvRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        //If our vertices arent the same
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }

        //FOURTH TRIANGLE 
        updatedVertices = new Vector3[] { rightMeshTriangle.Vertices[0], rightMeshTriangle.Vertices[1], vertRight };
        updatedNormals = new Vector3[] { rightMeshTriangle.Normals[0], rightMeshTriangle.Normals[1], normalRight };
        updatedUVs = new Vector2[] { rightMeshTriangle.UVs[0], rightMeshTriangle.UVs[1], uvRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        //If our vertices arent the same
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }
    }

    private static void FlipTriangle(MeshTriangle _triangle)
    {
        Vector3 temp = _triangle.Vertices[2];
        _triangle.Vertices[2] = _triangle.Vertices[0];
        _triangle.Vertices[0] = temp;

        temp = _triangle.Normals[2];
        _triangle.Normals[2] = _triangle.Normals[0];
        _triangle.Normals[0] = temp;

        (_triangle.UVs[2], _triangle.UVs[0]) = (_triangle.UVs[0], _triangle.UVs[2]);
    }

    public static void FillCut(List<Vector3> _addedVertices, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        List<Vector3> vertices = new();
        List<Vector3> polygon = new();

        for (int i = 0; i < _addedVertices.Count; i++)
        {
            if (!vertices.Contains(_addedVertices[i]))
            {
                polygon.Clear();
                polygon.Add(_addedVertices[i]);
                polygon.Add(_addedVertices[i + 1]);

                vertices.Add(_addedVertices[i]);
                vertices.Add(_addedVertices[i + 1]);

                EvaluatePairs(_addedVertices, vertices, polygon);
                Fill(polygon, _plane, _leftMesh, _rightMesh);
            }
        }
    }

    public static void EvaluatePairs(List<Vector3> _addedVertices, List<Vector3> _vertices, List<Vector3> _polygons)
    {
        bool isDone = false;
        while (!isDone)
        {
            isDone = true;
            for (int i = 0; i < _addedVertices.Count; i += 2)
            {
                if (_addedVertices[i] == _polygons[^1] && !_vertices.Contains(_addedVertices[i + 1]))
                {
                    isDone = false;
                    _polygons.Add(_addedVertices[i + 1]);
                    _vertices.Add(_addedVertices[i + 1]);
                }
                else if (_addedVertices[i + 1] == _polygons[^1] && !_vertices.Contains(_addedVertices[i]))
                {
                    isDone = false;
                    _polygons.Add(_addedVertices[i]);
                    _vertices.Add(_addedVertices[i]);
                }
            }
        }
    }

    private static void Fill(List<Vector3> _vertices, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        // Initialize a center position (vector) to zero. This will later hold the average position of all vertices.
        Vector3 centerPosition = Vector3.zero;

        // Sum up all the vertex positions. Iterate through all vertices in the _vertices list.
        for (int i = 0; i < _vertices.Count; i++)
        {
            centerPosition += _vertices[i];  // Add each vertex to centerPosition.
        }

        // Calculate the average position of the vertices by dividing the total sum by the number of vertices.
        centerPosition /= _vertices.Count;

        // Declare variables that will store displacement vectors and UV coordinates for the texture mapping.
        Vector2 uv1;
        Vector2 uv2;
        Vector2 centerUV = new(0.5f, 0.5f);

        Vector3[] leftNormals = { -_plane.normal, -_plane.normal, -_plane.normal };
        Vector3[] rightNormals = { _plane.normal, _plane.normal, _plane.normal };

        // Iterate through all vertices to create triangles. For each vertex, calculate the UVs and create two triangles.
        for (int i = 0; i < _vertices.Count; i++)
        {
            // Calculate the UV coordinates for the current vertex based on its displacement relative to the 'left' and 'up' axes.
            uv1 = new Vector2(
                0f,
                0f
            );

            uv2 = new Vector2(
                0f,
                0f
            );

            // Define the three vertices of the triangle: current vertex, next vertex, and the center position.
            Vector3[] vertices = { _vertices[i], _vertices[(i + 1) % _vertices.Count], centerPosition };

            if (Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).magnitude < Mathf.Epsilon)
            {
                continue;  // Skip degenerate triangles.
            }

            // Define the UVs for the triangle, with the third vertex (center) getting the UV coordinate (0.5, 0.5).
            Vector2[] uvs = { uv1, uv2, centerUV };

            // Create a new MeshTriangle using the vertices, normals, and UVs, and assign it to the 'left' mesh.
            // The subMeshCount + 1 ensures this triangle belongs to a new submesh.
            MeshTriangle currentTriangle = new(vertices, leftNormals, uvs, originalMesh.subMeshCount + 1);

            bool shouldFlipLeft = Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]), leftNormals[0]) < 0;

            // Check the orientation of the triangle using the cross product and the normal.
            // If the triangle is oriented incorrectly, flip it.
            if (shouldFlipLeft)
            {
                FlipTriangle(currentTriangle);  // Flip the triangle if it's facing the wrong way.
            }

            // Add the triangle to the _leftMesh.
            _leftMesh.AddTriangle(currentTriangle);

            // Create a new MeshTriangle with the outward-facing normals for the 'right' mesh.
            currentTriangle = new MeshTriangle(vertices, rightNormals, uvs, originalMesh.subMeshCount + 1);

            // Check the orientation of the triangle and flip if necessary for the 'right' mesh.
            if (!shouldFlipLeft)
            {
                FlipTriangle(currentTriangle);  // Flip if necessary.
            }

            // Add the triangle to the _rightMesh.
            _rightMesh.AddTriangle(currentTriangle);
        }
    }

    private static void CleanupSubmeshes(Mesh mesh, MeshRenderer meshRenderer, MeshFilter meshFilter, Material materialApllied)
    {
        // List to store valid submesh triangles and materials
        List<int[]> validSubmeshTriangles = new();

        // Loop through each submesh
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            // Get triangle indices for the submesh
            int[] submeshTriangles = mesh.GetTriangles(i);

            // Check if the submesh has at least one valid triangle (3 or more vertices)
            if (submeshTriangles.Length >= 3)
            {
                // Add this submesh's triangles to the valid list
                validSubmeshTriangles.Add(submeshTriangles);
            }
        }

        // If no submeshes are valid, log a warning and exit
        if (validSubmeshTriangles.Count == 0)
        {
            Debug.LogWarning("No valid submeshes found. The mesh may be completely empty.");
            return;
        }

        // Update the mesh with the valid submeshes
        mesh.subMeshCount = validSubmeshTriangles.Count;

        // Reassign the valid triangles to the mesh
        for (int i = 0; i < validSubmeshTriangles.Count; i++)
        {
            mesh.SetTriangles(validSubmeshTriangles[i], i);
        }

        // Get number of materials equal to submesh count
        Material[] mats = new Material[mesh.subMeshCount];
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = materialApllied;
        }

        // Assign the valid materials back to the MeshRenderer  
        meshRenderer.materials = mats;
        meshFilter.mesh = mesh;
    }

    private static void RecalculateUvs(Mesh mesh, GameObject obj)
    {
        // Get vertices and uvs
        Vector3[] _vertices = mesh.vertices;
        Vector2[] _uvs = new Vector2[_vertices.Length];

        // Get the bounds
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (minX > _vertices[i].x)
                minX = _vertices[i].x;
            if (minY > _vertices[i].y)
                minY = _vertices[i].y;
            if (maxX < _vertices[i].x)
                maxX = _vertices[i].x;
            if (maxY < _vertices[i].y)
                maxY = _vertices[i].y;
        }

        // Generate uvs
        for (int i = 0; i < _vertices.Length; i++)
        {
            Vector3 vertex = _vertices[i];

            _uvs[i] = new Vector2(Mathf.InverseLerp(minX, maxX, vertex.z),
                                  Mathf.InverseLerp(minY, maxY, vertex.y));

            _uvs[i].x = Mathf.Clamp(_uvs[i].x, 0.1f, 0.9f);
            _uvs[i].y = Mathf.Clamp(_uvs[i].y, 0.1f, 0.9f);
        }

        mesh.uv = _uvs;
    }

}

